using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions.Database;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide.Operations;
using Raven.Client.ServerWide;
using Raven.Embedded;

namespace InertGas.DataBase
{
    public class RavenDBRepository : IDataRepository, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public async Task Initialize(DataRepositoryConfiguration config)
        {
            EmbeddedServer.Instance.StartServer(new ServerOptions
            {
                DataDirectory = config.RootDirectory,
                ServerUrl = config.ServerUrl
            });

            // NOTE: DO NOT REMOVE. This line (seems to) ensure that the server is started
            // Perhaps there is a better way to achieve this
            _ = await EmbeddedServer.Instance.GetServerUriAsync();

            userStore_ = (new DocumentStore
            {
                Urls = new[] { config.ServerUrl },
                Database = config.UserDatabaseName
            }).Initialize();

            EnsureDatabaseExists(userStore_);

            dataStore_ = (new DocumentStore
            {
                Urls = new[] { config.ServerUrl },
                Database = config.DataDatabaseName
            }).Initialize();

            EnsureDatabaseExists(dataStore_);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                DisposeManaged();
            }

            DisposeUnmanaged();

            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RavenDBRepository()
        {
            Dispose(false);
        }

        private void DisposeManaged()
        {
            userStore_.Dispose();
            dataStore_.Dispose();
        }

        private void DisposeUnmanaged() { }

        public IEnumerable<User> GetUsers()
        {
            using var session = userStore_.OpenSession();
            return session.Query<User>().ToList();
        }

        public User UpsertUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using var session = userStore_.OpenSession();
            session.Store(user);
            session.SaveChanges();
            return session.Load<User>(user.Name);
        }

        public void DeleteUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using var session = userStore_.OpenSession();
            session.Delete(user.Id);
            session.SaveChanges();
        }

        public IEnumerable<User> SearchUserByName(string searchText)
        {
            if (searchText == "")
                return GetUsers();

            using var session = userStore_.OpenSession();
            return session.Query<User>()
                .Search(x => x.Name, searchText).ToList();
        }

        public IEnumerable<CollectedData> GetData()
        {
            using var session = dataStore_.OpenSession();
            return session.Query<CollectedData>().ToList();
        }

        public CollectedData UpsertData(CollectedData collectedData)
        {
            if (collectedData == null)
                throw new ArgumentNullException(nameof(collectedData));

            using var session = dataStore_.OpenSession();
            session.Store(collectedData);
            session.SaveChanges();
            return session.Load<CollectedData>(collectedData.Id);
        }

        public void DeleteData(CollectedData collectedData)
        {
            if (collectedData == null)
                throw new ArgumentNullException(nameof(collectedData));

            using var session = dataStore_.OpenSession();
            session.Delete(collectedData.Id);
            session.SaveChanges();
        }

        public IEnumerable<CollectedData> SearchDataByDate(DateTime fromDate, DateTime toDate)
        {
            if (toDate < fromDate)
                return GetData();

            using var session = dataStore_.OpenSession();
            return session.Query<CollectedData>()
                .Where(x => x.CreatedDate >= fromDate && x.CreatedDate <= toDate)
                .ToList();
        }

        private static void EnsureDatabaseExists(IDocumentStore store, string database = null, bool createDatabaseIfNotExists = true)
        {
            database ??= store.Database;

            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(database));

            try
            {
                store.Maintenance.ForDatabase(database).Send(new GetStatisticsOperation());
            }
            catch (DatabaseDoesNotExistException)
            {
                if (createDatabaseIfNotExists == false)
                    throw;

                try
                {
                    store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(database)));
                }
                catch (ConcurrencyException)
                {
                    // The database was already created before calling CreateDatabaseOperation
                }
            }
        }

        private IDocumentStore userStore_;
        private IDocumentStore dataStore_;
    }

}

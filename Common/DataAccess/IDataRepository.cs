using InertGas.Common.Model;

namespace InertGas.Common.DataAccess
{
    public interface IDataRepository
    {
        IEnumerable<User> GetUsers();

        User UpsertUser(User user);

        void DeleteUser(User user);

        IEnumerable<User> SearchUserByName(string searchText);

        IEnumerable<CollectedData> GetData();

        CollectedData UpsertData(CollectedData data);

        void DeleteData(CollectedData data);

        IEnumerable<CollectedData> SearchDataByDate(DateTime fromDate, DateTime toDate);

        void Dispose();
    }
}

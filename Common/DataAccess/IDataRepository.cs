using InertGas.Common.Model;

namespace InertGas.Common.DataAccess
{
    public interface IDataRepository
    {
        IEnumerable<User> GetUsers();

        User UpsertUser(User user);

        void DeleteUser(User user);

        IEnumerable<User> SearchPatientByName(string searchText);

        void Dispose();
    }
}

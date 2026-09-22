using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCore;

namespace IRepositoryAll
{
    public interface IUserStateTelegramRepository
    {
        public UserStateRegistration? Get(long id);
        public void Delete(long id);
        public long Update(UserStateRegistration registration);
        public long Create(UserStateRegistration registration);
    }
}

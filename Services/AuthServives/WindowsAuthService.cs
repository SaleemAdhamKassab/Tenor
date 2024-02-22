using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;

namespace Tenor.Services.AuthServives
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public interface IWindowsAuthService
    {
        string GetLoggedUser();
        List<string> GetUserGroupsFromAD(string userName);
        List<string> GetGroups();
        List<string> GetGroupsBySearch(string searchQuery);
        List<ADUser> GetUsers();
        List<ADUser> GetUsersBySearch(string searchQuery);
    }
    public class WindowsAuthService: IWindowsAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        public WindowsAuthService(IHttpContextAccessor httpContextAccessor, IConfiguration config)
         {
            _httpContextAccessor = httpContextAccessor;
            _config= config;
        }     
        public string GetLoggedUser()
        {
          
          string  user = _httpContextAccessor.HttpContext.User.Identity.Name;
          return user.Substring(user.IndexOf("\\")+1);

        }
        public List<string> GetUserGroupsFromAD(string userName)
        {
            string domainName = _config["Domain"];
            List<string> groupList= new List<string>();
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domainName, null, ContextOptions.SimpleBind, null, null))
            {
                UserPrincipal principal = new UserPrincipal(context);
                principal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName);
                if(principal==null)
                {
                    return null;
                }
                var groups = GroupPrincipal.FindByIdentity(context, domainName).GetGroups();
                foreach (var s in groups)
                {
                    groupList.Add(s.DisplayName);
                }
                return groupList;
            }
        }

        public List<string> GetGroups()
        {
            SearchResultCollection results;
            DirectorySearcher ds = null;
            DirectoryEntry de = new DirectoryEntry(GetCurrentDomainPath());
            ds = new DirectorySearcher(de);
            // Sort by name
            ds.Sort = new SortOption("name", SortDirection.Ascending);
            ds.PropertiesToLoad.Add("name");
            ds.PropertiesToLoad.Add("memberof");
            ds.PropertiesToLoad.Add("member");

            ds.Filter = "(&(objectCategory=Group))";
            results = ds.FindAll();
            var final = new List<string>();
            foreach (SearchResult sr in results)
            {
                if (sr.Properties["name"].Count > 0)
                {
                    final.Add(sr.Properties["name"][0].ToString());
                }
            }
            return final;
        }

        public List<string> GetGroupsBySearch(string searchQuery)
        {
            SearchResultCollection results;
            DirectorySearcher ds = null;
            DirectoryEntry de = new DirectoryEntry(GetCurrentDomainPath());
            ds = new DirectorySearcher(de);
            // Sort by name
            ds.Sort = new SortOption("name", SortDirection.Ascending);
            ds.PropertiesToLoad.Add("name");
            ds.PropertiesToLoad.Add("memberof");
            ds.PropertiesToLoad.Add("member");

            ds.Filter = "(&(objectCategory=Group)(name=*" + searchQuery + "*))";
            results = ds.FindAll();
            var final = new List<string>();
            foreach (SearchResult sr in results)
            {
                if (sr.Properties["name"].Count > 0)
                {
                    final.Add(de.Name.Replace("DC=", "").Trim() + "\\" + sr.Properties["name"][0].ToString());
                }
            }
            return final;
        }

        public List<ADUser> GetUsers()
        {
            SearchResultCollection results;
            DirectorySearcher ds = null;
            DirectoryEntry de = new DirectoryEntry(GetCurrentDomainPath());
            ds = new DirectorySearcher(de);
            // Sort by name
            ds.Sort = new SortOption("name", SortDirection.Ascending);
            ds.PropertiesToLoad.Add("name");
            ds.PropertiesToLoad.Add("sAMAccountName");

            ds.Filter = "(&(objectCategory=User)(objectClass=person))";
            results = ds.FindAll();
            var final = new List<ADUser>();
            foreach (SearchResult sr in results)
            {
                if (sr.Properties["sAMAccountName"].Count > 0)
                {
                    final.Add(new ADUser { Name = sr.Properties["sAMAccountName"][0].ToString(), Alias = sr.Properties["name"][0].ToString() });
                }
            }
            return final;
        }

        public List<ADUser> GetUsersBySearch(string searchQuery)
        {
            SearchResultCollection results;
            DirectorySearcher ds = null;
            DirectoryEntry de = new DirectoryEntry(GetCurrentDomainPath());
            ds = new DirectorySearcher(de);
            // Sort by name
            ds.Sort = new SortOption("name", SortDirection.Ascending);
            ds.PropertiesToLoad.Add("name");
            ds.PropertiesToLoad.Add("sAMAccountName");

            ds.Filter = "(&(objectCategory=User)(objectClass=person)(name=*" + searchQuery + "*))";
            results = ds.FindAll();
            var final = new List<ADUser>();
            foreach (SearchResult sr in results)
            {
                if (sr.Properties["name"].Count > 0)
                {
                    final.Add(new ADUser { Alias = sr.Properties["sAMAccountName"][0].ToString(), Name = sr.Properties["name"][0].ToString() });
                }
            }
            return final;
        }

        private string GetCurrentDomainPath()
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://RootDSE");

            return "LDAP://" + de.Properties["defaultNamingContext"][0].ToString();
        }

    }
}

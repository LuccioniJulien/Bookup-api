using System.Linq;
using BaseApi.Models;

namespace BaseApi.Dao {
    public static class TagsQuery {
        public static IQueryable<string> GetNamesTags (this IQueryable<Tag> query, string predicat) {
            return query.Where (x => x.Name.ToUpper ().Contains (predicat))
                .Select (x => x.Name);
        }
        public static IQueryable<string> GetNamesTags (this IQueryable<Tag> query) => query.Select (x => x.Name);
    }
}
using System.Runtime.Serialization;

namespace CatalogChallengeNet8.Application.Models
{
    public enum SortOrder
    {
        [EnumMember(Value = "asc")]
        Asc,

        [EnumMember(Value = "desc")]
        Desc
    }
}

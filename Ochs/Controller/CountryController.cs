using System.Collections.Generic;
using System.Web.Http;

namespace Ochs
{
    public class CountryController : ApiController
    {
        [HttpGet]
        public IDictionary<string, string> All() => Country.Countries;
    }
}
using ElasticsearchWithAspNetCore.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Diagnostics;

namespace ElasticsearchWithAspNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ElasticClient _elasticClient;
        public HomeController(ElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public IActionResult Index(string query)
        {
            ISearchResponse<Book> results;
            int size = 10;
            string marchQuery = query;

            if (!string.IsNullOrWhiteSpace(query))
            {
                results = _elasticClient.Search<Book>(s => s
                .From(0)
                .Size(size)
                .Query(q => q

                            //'*' özel karakteri ile arama yapmak için eklenen kısım. 
                            // Örn: *ing => ing ile bitenler.
                            .Bool(b => b
                                    .Should(m => m
                                        .Wildcard(wc => wc
                                        //.Field(p => p.Title).Value(AddWildcardToString(query, '*').ToLower()) // Bu satırı aktif edip alt satırı pasif edersek kullanıcı '*' karakterini kullanmadan alanın tamamında arama yapılır.
                                        .Field(p => p.Title).Value(query.ToLower())
                                        )
                                    )
                                )

                            || // "Match ve Bool tipindeki query'leri combine etmek için kullanıyoruz.

                    q.Match(t => t  // "Match" extension'u kullanıldığında arana değerin arana field'da olması yeterli. Contains, LIKE gibi.
                        .Field(f => f.Title)//Sorgunun hangi alana uygulanacağını belirten extension.
                        .Query(marchQuery)
                    )

                    //.Term(t => t // "Term" extension'u kullanılır ise aranan değerin bire bir aynı olması lazım. Id gibi.
                    //    .Value(query) // "Value, Field ve Term" birlikte kullanılıyor.
                    //    .Field(f => f.Isbn)
                    //)

                    )
                );
            }
            else
            {
                results = _elasticClient.Search<Book>(s => s
                            .From(0)
                            .Size(size)
                            .Query(q => q
                                .MatchAll()
                                )
                            );
            }

            return View(results);
        }

        private string AddWildcardToString(string value, char wildcard)
        {
            return string.Format("{0}{1}{2}", wildcard, value, wildcard).Replace(" ","");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

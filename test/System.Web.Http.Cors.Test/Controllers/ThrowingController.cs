namespace System.Web.Http.Cors
{
    [EnableCors("*", "*", "*")]
    public class ThrowingController : ApiController
    {
        public string Get()
        {
            throw new Exception();
        }
    }
}

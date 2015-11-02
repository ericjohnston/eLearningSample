using Learning.Data;
using Learning.ApiBase.Models;
using System.Web.Http;

namespace Learning.ApiBase
{
    public class BaseApiController : ApiController
    {
        private ILearningRepository _repo;
        private ModelFactory _modelFactory;

        public BaseApiController(ILearningRepository repo)
        {
            _repo = repo;
        }

        protected ILearningRepository TheRepository
        {
            get
            {
                return _repo;
            }
        }

        protected ModelFactory TheModelFactory
        {
            get
            {
                if(_modelFactory == null)
                {
                    _modelFactory = new ModelFactory(Request, TheRepository);
                }
                return _modelFactory;
            }
        }        
    }
}

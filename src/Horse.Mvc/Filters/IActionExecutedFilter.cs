using System.Threading.Tasks;
using Horse.Mvc.Controllers;

namespace Horse.Mvc.Filters
{
    /// <summary>
    /// Filter interface for Controller and Action Methods
    /// </summary>
    public interface IActionExecutedFilter
    {
        /// <summary>
        /// Called AFTER action method executed and BEFORE IControllerFilter objects' AfterAction methods are called.
        /// Changing Result of the FilterContext will be discarded.
        /// </summary>
        Task OnExecuted(IController controller, ActionDescriptor descriptor, IActionResult result, FilterContext context);
    }
}
using System.Data.Entity;

namespace WindowsFormsApp1
{
    internal class DataInitializer : DropCreateDatabaseIfModelChanges<BrowserContext>
    {
        protected override void Seed(BrowserContext context)
        {
            base.Seed(context);
        }
    }
}
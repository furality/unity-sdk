namespace Furality.SDK.Pages
{
    public abstract class MenuPage
    {
        protected MainWindow _mainWindow;

        protected MenuPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
        
        public abstract void Draw();
    }
}
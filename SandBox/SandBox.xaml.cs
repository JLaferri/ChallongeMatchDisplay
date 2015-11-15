using System;
using System.Windows.Input;

namespace PlayGround
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SandBox : System.Windows.Window
    {
        #region Private Properties

        // flag denoting whether goo is currently running
        private bool mIsGooRunning = false;

        #endregion

        #region Constructor

        public SandBox()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        
        /// <summary>
        /// Handle the event when the Goo button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MyButtonClicked(object sender, EventArgs e)
        {
            // if it is not running start it, otherwise stop it
            if (!mIsGooRunning)
                GooParticleSystem.Start();
            else
                GooParticleSystem.Stop();
            mIsGooRunning = !mIsGooRunning;
        }

        /// <summary>
        /// Handle the event when the mouse enters the Fire button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MyButtonMouseEnter(object sender, MouseEventArgs e)
        {
            // Start the particle system
            FireParticleSystem.Start();
        }

        /// <summary>
        /// Handle the event when the mouse leaves the Fire button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MyButtonMouseLeave(object sender, MouseEventArgs e)
        {
            // Stop the particle system
            FireParticleSystem.Stop();
        }

        #endregion
    }
}
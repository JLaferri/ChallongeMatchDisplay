using System.Windows;
using PlayGround.Engine.Controls;

namespace PlayGround.Engine.Emitters
{
    /// <summary>
    /// ========================================
    /// .NET Framework 3.0 Custom Control
    /// ========================================
    ///
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SandBox.Engine.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SandBox.Engine.Controls;assembly=SandBox.Engine.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file. Note that Intellisense in the
    /// XML editor does not currently work on custom controls and its child elements.
    ///
    ///     <MyNamespace:LineEmitter/>
    ///
    /// </summary>
    public class LineEmitter : Emitter
    {
        #region Properties

        /// <summary>
        /// Dependency Property which maintains the X1 position of the MeatballEmitter
        /// </summary>
        public static readonly DependencyProperty X1Property = DependencyProperty.Register(
           "X1", typeof(double), typeof(LineEmitter), new PropertyMetadata(0d));
        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }

        /// <summary>
        /// Dependency Property which maintains the Y1 position of the MeatballEmitter
        /// </summary>
        public static readonly DependencyProperty Y1Property = DependencyProperty.Register(
            "Y1", typeof(double), typeof(LineEmitter), new PropertyMetadata(0d));
        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }

        /// <summary>
        /// Dependency Property which maintains the X2 position of the MeatballEmitter
        /// </summary>
        public static readonly DependencyProperty X2Property = DependencyProperty.Register(
           "X2", typeof(double), typeof(LineEmitter), new PropertyMetadata(0d));
        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }

        /// <summary>
        /// Dependency Property which maintains the Y2 position of the MeatballEmitter
        /// </summary>
        public static readonly DependencyProperty Y2Property = DependencyProperty.Register(
            "Y2", typeof(double), typeof(LineEmitter), new PropertyMetadata(0d));
        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        static LineEmitter()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineEmitter), new FrameworkPropertyMetadata(typeof(LineEmitter)));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Add a particle to the system
        /// </summary>
        /// <param name="system"></param>
        /// <param name="particle"></param>
        public override void AddParticle(ParticleSystem system, Particle particle)
        {
            base.AddParticle(system, particle);

            // pick a random X between X1 and X2 
            // then get the corresponding y
            double x = ParticleSystem.random.NextDouble(X1, X2);
            particle.Position = new Point(x + ParticleSystem.random.NextDouble(MinPositionOffsetX, MaxPositionOffsetX), 
                LinearEquation(x) + ParticleSystem.random.NextDouble(MinPositionOffsetY, MaxPositionOffsetY)); 
        }

        /// <summary>
        /// Update the particle
        /// </summary>
        /// <param name="particle"></param>
        public override void UpdateParticle(Particle particle)
        {
            base.UpdateParticle(particle);                      

            // Find a new x and corresponding y 
            double x = ParticleSystem.random.NextDouble(X1, X2);
            particle.Position = new Point(x + ParticleSystem.random.NextDouble(MinPositionOffsetX, MaxPositionOffsetX), 
                LinearEquation(x) + ParticleSystem.random.NextDouble(MinPositionOffsetY, MaxPositionOffsetY));
            
        }

        #endregion

        #region Private Methods
         
        /// <summary>
        /// Find a y-coord on a line given an x-coord on the line
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private double LinearEquation(double x)
        {
            double m = 0;
            if ((X2 - X1) != 0)
                m = (Y2 - Y1) / (X2 - X1);
            double y = m * x + Y1;
            return y;
        }

        #endregion
    }
}

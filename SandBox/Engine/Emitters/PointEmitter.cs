using System.Windows;
using System.Windows.Controls;
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
    ///     <MyNamespace:PointEmitter/>
    ///
    /// </summary>
    public class PointEmitter : Emitter
    {
        #region Properties

        /// <summary>
        /// Dependency Property which maintains the X-Coord for this Emitter
        /// </summary>
        static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X", typeof(double), typeof(PointEmitter), new PropertyMetadata(0d));
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        /// <summary>
        /// Dependency Property which maintains the Y-Coord for this Emitter
        /// </summary>
        static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y", typeof(double), typeof(PointEmitter), new PropertyMetadata(0d));
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        #endregion

        #region Constructor
        
        /// <summary>
        /// 
        /// </summary>
        static PointEmitter()
        {
            
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PointEmitter), new FrameworkPropertyMetadata(typeof(PointEmitter)));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Init the width and height to one and set the canvas left and top 
        /// </summary>
        public override void EndInit()
        {
            base.EndInit();

            // Point emitter should always be 1x1
            this.Width = 1;
            this.Height = 1;

            Canvas.SetLeft(this, X);
            Canvas.SetTop(this, Y);
        }

        /// <summary>
        /// Add a particle to the system.
        /// </summary>
        /// <param name="system"></param>
        /// <param name="particle"></param>
        public override void AddParticle(ParticleSystem system, Particle particle) 
        {
            base.AddParticle(system, particle);
                        
            // overwrite the position based on the emitters X and Y position
            particle.Position = new Point(this.X + ParticleSystem.random.NextDouble(MinPositionOffsetX, MaxPositionOffsetX),
                this.Y + ParticleSystem.random.NextDouble(MinPositionOffsetY, MaxPositionOffsetY)); 
       
        }

        /// <summary>
        /// Update the particle
        /// </summary>
        /// <param name="particle"></param>
        public override void UpdateParticle(Particle particle)
        {
            base.UpdateParticle(particle);
            
            // Update the particles position
            particle.Position = new Point(this.X + ParticleSystem.random.NextDouble(MinPositionOffsetX, MaxPositionOffsetX),
                this.Y + ParticleSystem.random.NextDouble(MinPositionOffsetY, MaxPositionOffsetY));
         
        }

        #endregion
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using PlayGround.Engine.Forces;
using PlayGround.Engine.Emitters;

namespace PlayGround.Engine.Controls
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
    ///     <MyNamespace:ParticleSystem/>
    ///
    /// </summary>
    public class ParticleSystem : ItemsControl
    {
        #region Properties

        /// <summary>
        /// Static custom random number generator
        /// </summary>
        public static RandomNumberGenerator random = new RandomNumberGenerator(DateTime.Now.Millisecond);

        private double mThreshold = 1.0d; // the time the system will not run above
        private double mTime; // the total time ellapsed        
        private TimeSpan mPreviousTimeSpan = TimeSpan.Zero; // the previous timespan to render a fram
        private bool mIsRunning = false;
        //private Stopwatch mStopWatch; // the stopwatch which  // used to keep track of the framrate
        //private double mFrameCount = 0; // used to keep track of the frame count

        /// <summary>
        /// Dependency Property which maintains collection of emitter found in the system. The
        /// PropertyMetadata was removed do to problems running multiple instances of the system.
        /// </summary>
        public static readonly DependencyProperty EmittersProperty = DependencyProperty.Register(
            "Emitters", typeof(ObservableCollection<Emitter>), typeof(ParticleSystem));
            //new PropertyMetadata(new ObservableCollection<Emitter>()));
        public ObservableCollection<Emitter> Emitters
        {
            get { return (ObservableCollection<Emitter>)GetValue(EmittersProperty); }
            set { SetValue(EmittersProperty, value); }
        }

        /// <summary>
        /// Dependency Property which maintains collection of forces found in the system. The
        /// PropertyMetadata was removed do to problems running multiple instances of the system.
        /// </summary>
        public static readonly DependencyProperty ForcesProperty = DependencyProperty.Register(
            "Forces", typeof(ObservableCollection<Force>), typeof(ParticleSystem)); //,
            //new PropertyMetadata(new ObservableCollection<Force>()));
        public ObservableCollection<Force> Forces
        {
            get { return (ObservableCollection<Force>)GetValue(ForcesProperty); }
            set { SetValue(ForcesProperty, value); }
        }
        
        /*
        /// <summary>
        /// Dependency Property which maintains the framerate of the system. Used to display the 
        /// framerate.
        /// </summary>
        public static readonly DependencyProperty FrameRateProperty = DependencyProperty.Register(
            "FrameRate", typeof(String), typeof(ParticleSystem), new PropertyMetadata("0 fps"));
        public String FrameRate
        {
            get { return (String)GetValue(FrameRateProperty); }
            set { SetValue(FrameRateProperty, value); }            
        }
        */

        /// <summary>
        /// Dependency Property which maintains collection of particles found in the system. The
        /// PropertyMetadata was removed do to problems running multiple instances of the system.
        /// </summary>
        public static readonly DependencyProperty ParticlesProperty = DependencyProperty.Register(
            "Particles", typeof(ObservableCollection<Particle>), typeof(ParticleSystem)); //,
            //new PropertyMetadata(new ObservableCollection<Particle>()));
        public ObservableCollection<Particle> Particles
        {
            get { return (ObservableCollection<Particle>)GetValue(ParticlesProperty); }
            set { SetValue(ParticlesProperty, value); }
        }

        /// <summary>
        /// Dependency Property which maintains the global gravity force vector for the system
        /// </summary>
        public static readonly DependencyProperty GravityProperty = DependencyProperty.Register(
            "Gravity", typeof(Vector), typeof(ParticleSystem),
            new PropertyMetadata(new Vector()));
        public Vector Gravity
        {
            get { return (Vector)GetValue(GravityProperty); }
            set { SetValue(GravityProperty, value); }
        }

        /// <summary>
        /// Dependency Property which maintains the global drag force vector for the system
        /// </summary>
        public static readonly DependencyProperty DragProperty = DependencyProperty.Register(
            "Drag", typeof(Vector), typeof(ParticleSystem),
            new PropertyMetadata(new Vector()));
        public Vector Drag
        {
            get { return (Vector)GetValue(DragProperty); }
            set { SetValue(DragProperty, value); }
        }
                
        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        static ParticleSystem()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ParticleSystem), new FrameworkPropertyMetadata(typeof(ParticleSystem)));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Init the ParticleSystem
        /// </summary>
        public override void BeginInit()
        {
            base.BeginInit();

            // Init the ObservableCollections, if this is done when declaring the depenncy property
            // an error will occur when creating multiple particle systems in an application.
            SetValue(EmittersProperty, new ObservableCollection<Emitter>());
            SetValue(ForcesProperty, new ObservableCollection<Force>());
            SetValue(ParticlesProperty, new ObservableCollection<Particle>());
            
            //this.mStopWatch = new Stopwatch();

            // render every frame
            CompositionTarget.Rendering += UpdateRender;            
            
            //this.Loaded += new RoutedEventHandler(ParticleSystem_Loaded);
        }  

        #endregion

        #region Public Methods

        /// <summary>
        /// Update the particles in the particle system
        /// </summary>
        /// <param name="time"></param>
        public void Update(double time)
        {
            ScaleVectors(time);
            mTime += time; // update the time
        }

        /// <summary>
        /// Start the particle system
        /// </summary>
        public void Start()
        {
            Particles.Clear(); // clear all the particles 
            // Used to remove all forces that where Springs 
            //List<Force> lForce = new List<Force>(Forces); 
            //Forces = new ObservableCollection<Force>(lForce.FindAll(
            //    delegate(Force f) { return !f.GetType().Equals(typeof(Spring)); }));

            // for each emitter in the system generate particles
            foreach (Emitter emitter in Emitters)
            {
                emitter.GenerateParticles(this);
            }
            // start the system running
            mIsRunning = true; //mStopWatch.Start();
        }

        /// <summary>
        /// Stop the particle system
        /// </summary>
        public void Stop()
        {
            // stop the system running
            mIsRunning = false; //mStopWatch.Stop();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Event which fires every frame. Update the Particle System here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void UpdateRender(object sender, EventArgs e)
        {
            // Wait until the system is loaded
            if (this.IsLoaded)
            {
                // Used for calculating the frame rate
                //if (mFrameCount++ == 0)
                //    mStopWatch.Start();

                if (mIsRunning) //mStopWatch.IsRunning)
                {
                    // Used for calculating the frame rate
                    //mFrameCount++;
                    //long _framerate = (long)(mFrameCount / mStopWatch.Elapsed.TotalSeconds);
                    //FrameRate = String.Format("{0} fps {1}", (long)(mFrameCount / mStopWatch.Elapsed.TotalSeconds),
                    //    mStopWatch.Elapsed.Subtract(mPreviousTimeSpan).TotalSeconds);
                                 
                    // Update the time based on the current rendering time subtrated from the previous rendering time and 
                    // returned in seconds
                    double time = ((RenderingEventArgs)e).RenderingTime.Subtract(mPreviousTimeSpan).TotalSeconds; //(float)mStopWatch.Elapsed.Subtract(mPreviousTimeSpan).TotalSeconds;
                    if (time <= mThreshold)
                        Update(time); // update the system
                    else
                        Console.Out.WriteLine(String.Format("ParticleSystem is running above the threshold of {0} second{1}", mThreshold, mThreshold == 1 ? "" : "s"));                    

                    // save the rendering time
                    mPreviousTimeSpan = ((RenderingEventArgs)e).RenderingTime; //mStopWatch.Elapsed;
                }
            }
        }

        #endregion 

        #region Private Methods

        /// <summary>
        /// Scale the particle vectors for this system.
        /// </summary>
        /// <param name="time"></param>
        private void ScaleVectors(double time)
        {
            foreach (Particle particle in Particles)
            {
                // If the particle is alive
                if (particle.IsAlive) 
                {
                    // calculate the forces acting onthe particle at the current time
                    particle.Force = ComputeForces(particle, time);

                    // Update the velocity vectors for this particle based on the time
                    Vector v = particle.Velocity;
                    double vx = v.X + time * (particle.Force.X / particle.Mass);
                    double vy = v.Y + time * (particle.Force.Y / particle.Mass);
                    particle.Velocity = new Vector(vx, vy);

                    // If the particle is not an anchor update its position based on the time
                    if (!particle.IsAnchor)
                    {    
                        double px = particle.Position.X + time * particle.Velocity.X;
                        double py = particle.Position.Y + time * particle.Velocity.Y;
                        particle.Position = new Point(px, py);
                    }                   
                    particle.LifeSpan -= time; // update the particles lifespan
                }
            }           
        }

        /// <summary>
        /// Calculate all the forces acting on a particle at a given time
        /// </summary>
        /// <param name="particle"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private Vector ComputeForces(Particle particle, double time)
        {
            // set all the forces for a particle at a time and position
            double forceX = (particle.Mass * Gravity.X) - (particle.Velocity.X * Drag.X);
            double forceY = (particle.Mass * Gravity.Y) - (particle.Velocity.Y * Drag.Y);
            
            // If there are spring force connections on this particle then for every connection update
            // the forces acting upon the particle
            if (particle.Connections.Count > 0)
            {
                foreach (Spring s in particle.Connections)
                {
                    // apply the spring force   
                    Vector force = s.ApplyForce(particle);
                    forceX += force.X;
                    forceY += force.Y;
                }
            }
            // for every other force in the global list of forces, apply and update the force.
            foreach (Force f in Forces)
            {
                Vector force = f.ApplyForce(particle);
                forceX += force.X;
                forceY += force.Y;
            }
            return new Vector(forceX, forceY); // return the force vector.
        }
        
        #endregion

        #region Events

        /*
        private void ParticleSystem_Loaded(object sender, RoutedEventArgs e)
        {
            // Generate the particles for each emitter
            //foreach (Emitter emitter in Emitters)
            //{
            //    emitter.GenerateParticles(this);                
            //}     
            //mIsRunning = true; //Start();            
        } 
        */

        #endregion

    }
}

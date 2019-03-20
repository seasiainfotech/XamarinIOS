using Foundation;

using System;
using UIKit;
using Google.Maps;
using CoreGraphics;
using CoreLocation;
using MapKit;
using Plugin.Geolocator.Abstractions;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using ASEM.Social;

namespace ASEM.iOS
{
    public partial class SocialMapVC : BaseUIController, IAddUpdateSubject, SocialCircleListeners.IFriendsListViaDistanceListener, IRestSharpError, ICurrentLocation, SocialCircleListeners.ILocationUpdateListener
    {

        //Global Variables
        MapView mapView; 
        List<Marker> markers;
        int radius = 2;
        double radiusCustom = 2000;
        Google.Maps.Circle circle;
        CameraPosition camera;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ASEM.iOS.SocialMapVC"/> class.
        /// </summary>
        /// <param name="handle">Handle.</param>
        public SocialMapVC(IntPtr handle) : base(handle)
        {

        }


      #region View cycle
        /// <summary>
        /// Views the did load.
        /// </summary>

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            unHideNavigationBar(this.NavigationController);
            setupNavigationBackground(this.NavigationController);
            this.NavigationController.NavigationBar.TintColor = UIColor.White;
            this.Title = "SOCIAL CIRCLE"; 
            GlobalLocation globalLocation = new GlobalLocation(this, true);
            GradientBackgroundForButton(msgButton);
            msgButton.Layer.CornerRadius = 4;
            msgButton.ClipsToBounds = true;
            CornerRadius(35, addButton);
            NavigationController.NavigationBar.Translucent = false;
            if (!string.IsNullOrEmpty(Settings.UserLatSettings))
            {
                GlobalLocation globalLocation_Local = new GlobalLocation(this, true);
                camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(Settings.UserLatSettings),
                                                   longitude: Convert.ToDouble(Settings.UserLongSettings), zoom: 12, bearing: 44, viewingAngle: 10);
                mapView = MapView.FromCamera(CGRect.Empty, camera);
                if (mapView != null)
                {
                    mapView.MyLocationEnabled = true;
                    mapView.Animate(camera);
                }
            }
            else
            {
                camera = CameraPosition.FromCamera(latitude: 30.7108, longitude: 76.7094, zoom: 12, bearing: 44, viewingAngle: 10);
                mapView = MapView.FromCamera(CGRect.Empty, camera);
                mapView.MyLocationEnabled = true;
            }

            var fitBoundsButton = new UIBarButtonItem("", UIBarButtonItemStyle.Plain, DidTapFitBounds);
            fitBoundsButton.Image = UIImage.FromBundle("Image-1");
            NavigationItem.RightBarButtonItem = fitBoundsButton;
            customButton1.Frame = new CGRect(8, 5, 45, 45);
            customButton2.Frame = new CGRect(View.Frame.X + View.Frame.Width - 53, 5, 45, 45);
            customButton3.Frame = new CGRect(View.Frame.X + View.Frame.Width - 53, customButton2.Frame.Y + customButton2.Frame.Height + 5, 45, 45);
            msgButton.Frame = new CGRect(20, View.Frame.Height - 140, View.Frame.Width - 40, 45);
            addButton.Frame = new CGRect(View.Frame.X + View.Frame.Width - 80, View.Frame.Height - 230, addButton.Frame.Width, addButton.Frame.Height);
            mapView.Add(msgButton);
            mapView.Add(addButton);
            mapView.Add(customButton1);
            mapView.Add(customButton2);
            mapView.Add(customButton3);
            View = mapView;
         
            mapView.Delegate = new CustomMapDelegate(mapView); //Call Delegate of MapView
            markers = new List<Marker>(); // Initialize marker list
            HandleClicks();              //Button Clicks
            CreateCircle();             //Circle with static radius
        }
         
        /// <summary>
        /// Views the will appear.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            GlobalLocation globalLocation = new GlobalLocation(this, true); //Current location
            FetchFriends();  //API call
        }

    #endregion

        #region Event Handlers
        /// <summary>
        /// Adds the circle action.
        /// </summary>
        /// <param name="sender">Sender.</param>
        partial void AddCircleAction(UIButton sender)
        {
            var obj = getViewController(Global.kStoryBoards.KSocialCircleVC.KAddCircleVC, Global.kStoryBoards.KSocialCircle) as AddCircleVC;
            this.NavigationController.PushViewController(obj, true);
        }
        /// <summary>
        /// Okaies the button clicked.
        /// </summary>
        /// <param name="enteredText">Entered text.</param>
        /// <param name="index">Index.</param>
        public void OkayButtonClicked(string enteredText, int index)
        {
            if (enteredText != "")
            {
                radius = int.Parse(enteredText); //For Api use
                radiusCustom = radius * 1000;    //For creating circle 
                FetchFriends();                  //Api call
            }
        }

        /// <summary>
        /// Deletes the button clicked.
        /// </summary>
        /// <param name="index">Index.</param>
        public void DeleteButtonClicked(int index)
        {

        }
        /// <summary>
        /// Edits the button clicked.
        /// </summary>
        /// <param name="index">Index.</param>
        public void EditButtonClicked(int index)
        {

        }

        /// <summary>
        /// Dids the tap fit bounds.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void DidTapFitBounds(object sender, EventArgs e)
        {
            var obj2 = getViewController(Global.kStoryBoards.KSocialCircleVC.KCircleRequestVC, Global.kStoryBoards.KSocialCircle) as CircleRequestVC;
            this.NavigationController.PushViewController(obj2, true);

        }

        /// <summary>
        /// Handles the clicks.
        /// </summary>
        private void HandleClicks()
        {
            msgButton.TouchUpInside += (sender, e) =>
            {
                ShowAlert("Coming Soon");
            };

            customButton2.TouchUpInside += (sender, e) =>
            {
                AnimateCamera();

            };
            customButton3.TouchUpInside += (sender, e) =>
            {
                AlertViewForAddUpdate("", "", this, "Enter Distance(in kms)", "Done");

            };
            customButton1.TouchUpInside += (sender, e) =>
            {


                var obj = getViewController(Global.kStoryBoards.KSocialCircleVC.KMyCircleListVC, Global.kStoryBoards.KSocialCircle) as MyCircleListVC;
                this.NavigationController.PushViewController(obj, true);

            };
        }
        #endregion

        #region Animate Camera
        /// <summary>
        /// Animates the camera.
        /// </summary>
        public void AnimateCamera()
        {
            GlobalLocation globalLocation_Local = new GlobalLocation(this, true);

            if (!string.IsNullOrEmpty(Settings.UserLatSettings)) {

                camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(Settings.UserLatSettings),
                                             longitude: Convert.ToDouble(Settings.UserLongSettings), zoom: 12, bearing: 44, viewingAngle: 10);
                if (mapView != null)
                {
                    mapView.MyLocationEnabled = true;
                    mapView.Animate(camera);
                }
            }
        }
        #endregion

        #region Circle
        /// <summary>
        /// Creates the circle.
        /// </summary>
        public void CreateCircle()
        {
            if (mapView != null)
            {
                if (circle != null)
                {
                    circle.Map = null;
                }
                if (!string.IsNullOrEmpty(Settings.UserLatSettings))
                {
                    circle = new Circle
                    {
                        FillColor = UIColor.FromRGB(215, 233, 241).ColorWithAlpha(2f),
                        Position = new CLLocationCoordinate2D(Convert.ToDouble(Settings.UserLatSettings), Convert.ToDouble(Settings.UserLongSettings)),
                        Radius = radiusCustom,
                        StrokeColor = UIColor.FromRGB(47, 117, 169),
                        Map = mapView

                    };
                }
            }
        }

        #endregion

        #region Markers
        /// <summary>
        /// Creates the pins.
        /// </summary>
        /// <param name="currentuser">Currentuser.</param>
        public void CreatePins(User currentuser)
        {
            string strImage = "";
            var keys = new[]
                    {
                        new NSString("name"),
                        new NSString("miles"),
                        new NSString("image")
                        };

            if (currentuser.ImgUrl != null)
            {
                strImage = currentuser.ImgUrl.Trim();
            }
            var objects = new NSObject[]
             {
                new NSString(currentuser.FriendName),
                new NSString(currentuser.Miles.ToString()),
                new NSString(strImage)

             };

            var dicionary = new NSDictionary<NSString, NSObject>(keys, objects);
            //// Create a list of markers, adding the Sydney marker.
            CLLocationCoordinate2D cLLocationCoordinate2D = new CLLocationCoordinate2D(Convert.ToDouble(currentuser.Latitude), Convert.ToDouble(currentuser.Longitude));
            var marker = new Marker()
            {
                Title = string.Format("Marker at: {0}, {1}", currentuser.Latitude, currentuser.Longitude),
                Position = cLLocationCoordinate2D,
                AppearAnimation = MarkerAnimation.Pop,
                UserData = dicionary,
                Map = mapView
            };

            // Add the new marker to the list of markers.
            markers.Add(marker);

        }

        /// <summary>
        /// Shows the markers on map.
        /// </summary>
        /// <param name="friendList">Friend list.</param>
        private void ShowMarkersOnMap(List<User> friendList)
        {
            try
            {
                if (friendList.Count > 0)
                {
                    InvokeOnMainThread(() =>
                    {
                        mapView.Clear();
                        foreach (var user in friendList)
                        {
                            var latLng = new CLLocationCoordinate2D(double.Parse(user.Latitude), double.Parse(user.Longitude));
                            double price;
                            bool isDouble = Double.TryParse(user.Miles.ToString(), out price);
                            if (isDouble)
                            {
                                user.Distance = user.Miles;
                            }
                            CreatePins(user); // Create Annotations on map
                        }
                        CreateCircle();   // Radius circle on map
                        AnimateCamera(); //For focusing on current location
                    });
                }
                else
                {
                    CreateCircle();  // Radius circle on map
                }
            }
            catch (System.Exception e)
            {
                PrintLogDetails.instance.PrintLogDeatails("Map", "Friendlist", e.Message);
            }

        }
        #endregion


        /// <summary>
        /// Fetchs the friends.
        /// </summary>
        #region Api 
        public void FetchFriends()
        {

            FetchFriendsListViaDistanceApi fetchFriendsListViaDistanceApi = new FetchFriendsListViaDistanceApi(this, this, this);
            fetchFriendsListViaDistanceApi.GetFriends(Settings.UserLatSettings, Settings.UserLongSettings, (int)radius);

        }

        /// <summary>
        /// Friendses the location.
        /// </summary>
        public void FriendsLoc()
        {
            if (!string.IsNullOrEmpty(Settings.UserIdSettings))
            {
                UpdateLocationApi updateLocationApi = new UpdateLocationApi(this, this, this);
                updateLocationApi.GetRequests(Settings.UserIdSettings.ToInt(), Settings.UserLatSettings, Settings.UserLongSettings);
            }
        }

        /// <summary>
        /// Friendses the list via dist result.
        /// </summary>
        /// <param name="friendsListModel">Friends list model.</param>
        public void FriendsListViaDistResult(SearchUsersModel friendsListModel)
        {
            InvokeOnMainThread(() =>
            {
                if (friendsListModel.User != null && friendsListModel.User.Count > 0)
                    ShowMarkersOnMap(friendsListModel.User);
                else{
                    mapView.Clear();
                    if (radius == 2)
                        ShowAlert(SocialStrings.CommonStrings.NoFriends);
                    else
                        ShowAlert(SocialStrings.CommonStrings.NoFriendsInArea);
                    CreateCircle();
                    AnimateCamera();
                }
                  

            });
        }

        /// <summary>
        /// Shows the rest sharp service error.
        /// </summary>
        /// <param name="error">Error.</param>
        public void ShowRestSharpServiceError(string error)
        {

        }
        #endregion


        #region Location
        /// <summary>
        /// Currents the location.
        /// </summary>
        /// <param name="location">Location.</param>
        public void CurrentLocation(CLLocation location)
        {
            if (location != null)
            {
                Settings.UserLatSettings = location.Coordinate.Latitude.ToString();
                Settings.UserLongSettings = location.Coordinate.Longitude.ToString();
                mapView.SetMinMaxZoom(5, 18);
                FriendsLoc();
            }
        }

        /// <summary>
        /// Failded the specified error.
        /// </summary>
        /// <param name="error">Error.</param>
        public void Failded(string error)
        {

        }

        /// <summary>
        /// Locations the update result.
        /// </summary>
        /// <param name="connectionModel">Connection model.</param>
        public void LocationUpdateResult(ConnectionModel connectionModel)
        {

        }
        #endregion

        /// <summary>
        /// Dids the receive memory warning.
        /// </summary>
        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.  
        }
    }



}
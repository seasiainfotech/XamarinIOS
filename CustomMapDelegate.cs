using Google.Maps;
using UIKit;
using CoreGraphics;
using Foundation;
using SDWebImage;
using System;

namespace ASEM.iOS
{
    internal class CustomMapDelegate : MapViewDelegate
    {
        //Objects
        private MapView mapView;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ASEM.iOS.CustomMapDelegate"/> class.
        /// </summary>
        /// <param name="mapView">Map view.</param>
        public CustomMapDelegate(MapView mapView)
        {
            this.mapView = mapView;
        }

     #region Delegate Methods
        /// <summary>
        /// Markers the info window.
        /// </summary>
        /// <returns>The info window.</returns>
        /// <param name="mapView">Map view.</param>
        /// <param name="marker">Marker.</param>
        public override UIView MarkerInfoWindow(MapView mapView, Marker marker)
        {
            //Initialize Dictionary
            NSDictionary dict = new NSDictionary();
            dict = marker.UserData as NSDictionary;

            if (dict != null)
            {
                //PopUp Window
                UIView viewCustom = new UIView();
                viewCustom.Frame = new CGRect(0, 10, 200, 70);
                viewCustom.BackgroundColor = UIColor.White;
                viewCustom.Layer.BorderColor = UIColor.FromRGB(47, 117, 169).CGColor;
                viewCustom.Layer.BorderWidth = 0.6f;
                SetCardView(viewCustom);
                //Image of friend
                UIImageView imgView = new UIImageView();
                imgView.Frame = new CGRect(8, 10, 50, 50);
                imgView.Image = UIImage.FromBundle("socialIcon");
                imgView.Layer.CornerRadius = 25;
                imgView.Layer.MasksToBounds = true;
                imgView.Layer.BorderWidth = 1;
                imgView.Layer.BorderColor = UIColor.FromRGB(47, 117, 169).CGColor;
                imgView.ContentMode = UIViewContentMode.ScaleAspectFit;

                if (dict.ValueForKey((NSString)"image") != null && dict.ValueForKey((NSString)"image").ToString() != "")
                {
                    var str = dict.ValueForKey((NSString)"image");
                    if (str != null)
                    {
                        imgView.SetImage(new NSUrl(str.ToString()));
                    }
                    else
                    {
                        imgView.Image = UIImage.FromBundle("chat_userIcon");
                    }
                }
                else
                {
                    imgView.Image = UIImage.FromBundle("chat_userIcon");
                }

                viewCustom.Add(imgView);

                //Name of Friend
                UILabel lblTitle = new UILabel();
                lblTitle.Frame = new CGRect(imgView.Frame.X + imgView.Frame.Width + 10, 5, 100, 25);
                lblTitle.Font = lblTitle.Font.WithSize(12f);
                var strName = dict.ValueForKey((NSString)"name");
                lblTitle.Text = strName.ToString();
                viewCustom.Add(lblTitle);
                //Location of friend
                UILabel lblSubTitle = new UILabel();
                lblSubTitle.Frame = new CGRect(imgView.Frame.X + imgView.Frame.Width + 7, lblTitle.Frame.Y + lblTitle.Frame.Height, 100, 30);
                lblSubTitle.Font = lblSubTitle.Font.WithSize(12f);
                var strMiles = dict.ValueForKey((NSString)"miles");

                try
                {
                    var roundDist = Math.Round(double.Parse(strMiles.ToString()));
                    lblSubTitle.Text = "(" + roundDist.ToString() + "km" + ")";
                }
                catch (Exception e)
                {
                    Console.WriteLine("error");
                    lblSubTitle.Text = strMiles.ToString();
                }
                viewCustom.Add(lblSubTitle);
                return viewCustom;
            }
            else
            {
                return base.MarkerInfoContents(mapView, marker);
            }
        }


        /// <summary>
        /// Tappeds the marker.
        /// </summary>
        /// <returns><c>true</c>, if marker was tappeded, <c>false</c> otherwise.</returns>
        /// <param name="mapView">Map view.</param>
        /// <param name="marker">Marker.</param>
        public override bool TappedMarker(MapView mapView, Marker marker)
        {
            mapView.SelectedMarker = marker;
            return true;
        }

        #endregion

        #region Customize the pop up
        /// <summary>
        /// Sets the card view.
        /// </summary>
        /// <param name="view">View.</param>
        public void SetCardView(UIView view)  
        {
            var opacity = 0.8f;
            view.Layer.MasksToBounds = true;
            view.Layer.ShadowOffset = new CoreGraphics.CGSize(5, 5);
            view.Layer.CornerRadius = 8;
            view.Layer.ShadowRadius = 8;
            view.Layer.ShadowOpacity = opacity;
        }
        #endregion

    }
}
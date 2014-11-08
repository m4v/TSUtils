//
//  TrackingStationUtils.cs
//
//  Author:
//       Elián Hanisch <lambdae2@gmail.com>
//
//  Copyright (c) 2014 Elián Hanisch 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrackingStationUtils
{
	[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
	public class TrackingStationUtils : MonoBehaviour
	{
		enum Modes { ShowParts, ShowRes, ShowOrbit };

		public static TrackingStationUtils Instance;

		Modes mode;
		Vessel vessel;
		CelestialBody planet;
		int windowsId;
		bool guiEnabled = false;
		Dictionary<string, string> itemList;
		Rect winRect = new Rect(210, 100, 100, 20);
		Vector2 scrollPosition = Vector2.zero;

		Vector2 colSize1;
		Vector2 colSize2;
		float rows;

		const float sep1 = 38f;
		const float sep2 = 4.1f;
		const float sep3 = 4.8f;
		const float maxRows = 15;

		TrackingStationUtils ()
		{
			Instance = this;
		}

		void Awake ()
		{
			windowsId = gameObject.GetInstanceID();
		}

		void Update ()
		{
			Vessel oldVessel = vessel;
			CelestialBody oldPlanet = planet;
			vessel = isVesselSelected () ? PlanetariumCamera.fetch.target.vessel : null;
			planet = isPlanetSelected () ? PlanetariumCamera.fetch.target.celestialBody : null;
			if ((oldVessel != vessel) || (oldPlanet != planet)) {
				itemList = null;
			}
		}

		static bool isVesselSelected ()
		{
			return (PlanetariumCamera.fetch != null)
			&& (PlanetariumCamera.fetch.target != null)
			&& (PlanetariumCamera.fetch.target.vessel != null);
		}

		static bool isPlanetSelected ()
		{
			return (PlanetariumCamera.fetch != null)
			&& (PlanetariumCamera.fetch.target != null)
			&& (PlanetariumCamera.fetch.target.celestialBody != null);
		}

		void OnGUI ()
		{
			if (!guiEnabled) {
				return;
			}
			switch (mode) {
			case Modes.ShowOrbit:
				if (!isVesselSelected () && !isPlanetSelected ()) {
					return;
				}
				break;
			case Modes.ShowParts:
			case Modes.ShowRes:
				if (!isVesselSelected ()) {
					return;
				}
				break;
			}
			GUI.skin = HighLogic.Skin;
			GUI.skin.label.wordWrap = false;

			if (Event.current.type == EventType.Layout) {
				winRect.width = 100;
				winRect.height = 20;
			}

			string title = "TSUtils";
			switch (mode) {
			case Modes.ShowParts:
				title = "Parts list";
				break;
			case Modes.ShowRes:
				title = "Resource list";
				break;
			case Modes.ShowOrbit:
				title = "Orbit parameters";
				break;
			}
			winRect = GUILayout.Window (windowsId, winRect, drawWindow, title);
		}

		void drawWindow (int id)
		{
			if (itemList == null)   {
				switch (mode) {
				case Modes.ShowParts:
					itemList = getPartList (vessel);
					break;
				case Modes.ShowRes:
					itemList = getResourceList (vessel);
					break;
				case Modes.ShowOrbit:
					if (isVesselSelected ()) {
						itemList = getOrbitParams (vessel.orbit);
					} else if (isPlanetSelected ()) {
						itemList = getOrbitParams (planet.orbit);
					}
					break;
				}
				rows = Mathf.Clamp (itemList.Count, 0, maxRows);
				updateColumnSizes ();
			}
			if (rows > 0) {
				scrollPosition = GUILayout.BeginScrollView (scrollPosition, 
															GUILayout.Width (colSize1.x + colSize2.x + sep1), 
															GUILayout.Height (((colSize1.y + sep2) * rows) + sep3));
				{
					GUILayout.BeginVertical ();
					{
						foreach (var pair in itemList) {
							GUILayout.BeginHorizontal ();
							{
								GUILayout.Label (pair.Key, GUILayout.Width (colSize1.x));
								GUILayout.Label (pair.Value, GUILayout.Width (colSize2.x));
							}
							GUILayout.EndHorizontal ();
						}
					}
					GUILayout.EndVertical ();
				}
				GUILayout.EndScrollView ();
			}
			if (GUILayout.Button ("Close")) {
				guiEnabled = false;
			}
			GUI.DragWindow ();
		}

		Dictionary<string, string> getPartList (Vessel vssl)
		{
			var dict = new Dictionary<string, int> ();

			foreach (var part in vssl.protoVessel.protoPartSnapshots) {
				string title = part.partInfo.title;
				int count = 0;
				if (dict.TryGetValue (title, out count)) {
					count += 1;
					dict [title] = count;
				} else {
					dict [title] = 1;
				}
			}
			var dict2 = new Dictionary<string, string> ();
			foreach (var item in dict) {
				if (item.Value > 1) {
					dict2 [item.Key] = String.Format ("x{0}", item.Value);
				} else {
					dict2 [item.Key] = " ";
				}
			}
			return dict2;
		}

		Dictionary<string, string> getResourceList (Vessel vssl)
		{
			var dict = new Dictionary<string, float> ();

			foreach (var part in vssl.protoVessel.protoPartSnapshots) {
				foreach (var res in part.resources) {
					string rsrc = res.resourceName;
					string value = res.resourceValues.GetValue ("amount");
					float amount = 0;
					amount = String.IsNullOrEmpty (value) ? 0 : float.Parse (value);
					if (amount > 0.01) {
						float value2 = 0;
						if (dict.TryGetValue (rsrc, out value2)) {
							dict [rsrc] = value2 + amount;
						} else {
							dict [rsrc] = amount;
						}
					}
				}
			}
			var dict2 = new Dictionary<string, string> ();
			foreach (var item in dict) {
				dict2 [item.Key] = item.Value.ToString ("0.##");
			}
			return dict2;
		}

		Dictionary<string, string> getOrbitParams (Orbit orbit)
		{
			var dict = new Dictionary<string, string> ();
			dict ["Semi-major axis"] = Utils.format_SI (orbit.semiMajorAxis);
			dict ["Eccentricity"] = orbit.eccentricity.ToString ("0.####");
			dict ["Inclination"] = Utils.format_angle (orbit.inclination);
			dict ["Argument of Pe"] = Utils.format_angle (orbit.argumentOfPeriapsis);
			dict ["Longitude of AN"] = Utils.format_angle (orbit.LAN);
			dict ["True anomaly"] = Utils.format_angle (orbit.trueAnomaly);
			dict ["Orbital period"] = Utils.format_time (orbit.period);

			return dict;
		}

		void updateColumnSizes() {
			colSize1 = Vector2.zero;
			colSize2 = Vector2.zero;
			foreach (var item in itemList) {
				Vector2 size1 = GUI.skin.label.CalcSize (new GUIContent (item.Key));
				if (colSize1.x < size1.x) {
					colSize1 = size1;
				}
				Vector2 size2 = GUI.skin.label.CalcSize (new GUIContent (item.Value));
				if (colSize2.x < size2.x) {
					colSize2 = size2;
				}
			}
		}

		public static Vessel getSelectedVessel ()
		{
			return Instance.vessel;
		}

		public static CelestialBody getSelectedPlanet ()
		{
			return Instance.planet;
		}

		public static void ShowParts()
		{
			Instance.guiEnabled = true;
			Instance.mode = Modes.ShowParts;
			Instance.itemList = null;
		}

		public static void ShowVesselInfo ()
		{
			Instance.guiEnabled = true;
			Instance.mode = Modes.ShowRes;
			Instance.itemList = null;
		}

		public static void ShowOrbit()
		{
			Instance.guiEnabled = true;
			Instance.mode = Modes.ShowOrbit;
			Instance.itemList = null;
		}
	}
}


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
	interface ITwoValueColumn {
		string Name { get; }
		string Value { get; }
	}
		
	public class ResourceCount : ITwoValueColumn
	{
		List<ProtoPartResourceSnapshot> resources = new List<ProtoPartResourceSnapshot> ();
		string resourceName;

		public ResourceCount (string name) {
			resourceName = name;
		}

		public ResourceCount (ProtoPartResourceSnapshot res)
		{
			resourceName = res.resourceName;
			resources.Add (res);
		}

		#region ITwoValueColumn implementation
		public string Name {
			get { return resourceName; }
		}

		public string Value {
			get { return Amount.ToString ("0.##"); }
		}
		#endregion

		public void Add (ProtoPartResourceSnapshot resource) {
			resources.Add (resource);
		}

		public float Amount {
			get {
				float total = 0;
				foreach (var r in resources) {
					string s = r.resourceValues.GetValue ("amount");
					float v = string.IsNullOrEmpty (s) ? 0 : float.Parse (s);
					total += v;
				}
				return total;
			}
		}

	}

	public class StringPair : ITwoValueColumn
	{
		string name, value;

		public StringPair (string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		#region ITwoValueColumn implementation
		public string Name {
			get { return name; }
		}
		public string Value {
			get { return value; }
		}
		#endregion
	}

	public class DynamicValueItem : ITwoValueColumn
	{
		string name;
		Func<string> getValueMethod;

		public DynamicValueItem(string name, Func<string> method) {
			this.name = name;
			this.getValueMethod = method;
		}

		#region ITwoValueColumn implementation
		public string Name {
			get { return name; }
		}
		public string Value {
			get { return getValueMethod ();	}
		}
		#endregion
	}

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
		List<ITwoValueColumn> itemList;
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
						foreach (var item in itemList) {
							GUILayout.BeginHorizontal ();
							{
								GUILayout.Label (item.Name, GUILayout.Width (colSize1.x));
								GUILayout.Label (item.Value, GUILayout.Width (colSize2.x));
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

		List<ITwoValueColumn> getPartList (Vessel vssl)
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
			var list = new List<ITwoValueColumn> ();
			foreach (var item in dict) {
				if (item.Value > 1) {
					list.Add(new StringPair(item.Key, String.Format ("x{0}", item.Value)));
				} else {
					list.Add(new StringPair(item.Key, " "));
				}
			}
			return list;
		}

		List<ITwoValueColumn> getResourceList (Vessel vssl)
		{
			var dict = new Dictionary<string, ResourceCount> ();
			var list = new List<ITwoValueColumn> ();

			foreach (var part in vssl.protoVessel.protoPartSnapshots) {
				foreach (var res in part.resources) {
					string name = res.resourceName;
					ResourceCount item;
					if (dict.TryGetValue (name, out item)) {
						item.Add (res);
					} else {
						var rc = dict [name] = new ResourceCount(res);
						list.Add (rc);
					}
				}
			}

			return list;
		}

		List<ITwoValueColumn> getOrbitParams (Orbit orbit)
		{
			var list = new List<ITwoValueColumn> ();
			list.Add(new DynamicValueItem("Semi-major axis", () => Utils.format_SI (orbit.semiMajorAxis)));
			list.Add(new DynamicValueItem("Eccentricity", () => orbit.eccentricity.ToString ("0.####")));
			list.Add(new DynamicValueItem("Inclination", () => Utils.format_angle (orbit.inclination)));
			list.Add(new DynamicValueItem("Argument of Pe", () => Utils.format_angle (orbit.argumentOfPeriapsis)));
			list.Add(new DynamicValueItem("Longitude of AN", () => Utils.format_angle (orbit.LAN)));
			list.Add(new DynamicValueItem("True anomaly", () => Utils.format_angle (orbit.trueAnomaly)));
			list.Add(new DynamicValueItem("Orbital period", () => Utils.format_time (orbit.period)));

			return list;
		}

		void updateColumnSizes() {
			colSize1 = Vector2.zero;
			colSize2 = Vector2.zero;
			foreach (var item in itemList) {
				Vector2 size1 = GUI.skin.label.CalcSize (new GUIContent (item.Name));
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


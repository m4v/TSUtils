//
//  AppLauncher.cs
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
using UnityEngine;

namespace TrackingStationUtils
{
	/* AppLauncher doesn't work in Tracking Station */
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class AppLauncher : MonoBehaviour
	{
		Texture2D icon;
		ApplicationLauncherButton button;
		Action applicationLauncherCallback;

		void Awake ()
		{
			icon = new Texture2D (38, 38);
			addButton ();
		}

		void onApplicationLauncherReady ()
		{
			if (applicationLauncherCallback != null) {
				applicationLauncherCallback ();
				applicationLauncherCallback = null;
			}
			GameEvents.onGUIApplicationLauncherReady.Remove (onApplicationLauncherReady);
		}

		void _addButton () {
			if (button == null) {
				button = ApplicationLauncher.Instance.AddModApplication (onTrue, onFalse, null, null, null, null,
					ApplicationLauncher.AppScenes.ALWAYS, icon);
			}
		}

		void addButton ()
		{
			if (ApplicationLauncher.Ready) {
				_addButton ();
			} else {
				GameEvents.onGUIApplicationLauncherReady.Add (onApplicationLauncherReady);
				applicationLauncherCallback = _addButton;
			}
		}

		void onTrue ()
		{
			PlanetariumCamera planetarium = PlanetariumCamera.fetch;
			if ((planetarium != null) && (planetarium.target != null) && (planetarium.target.vessel != null)) {
				planetarium.target.vessel.RenameVessel ();
				print (planetarium.target.vessel.name);
			} else {
				print ("null");
			}
		}

		void onFalse ()
		{
			onTrue ();
		}
	}
}


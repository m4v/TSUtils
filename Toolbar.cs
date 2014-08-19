//
//  Toolbar.cs
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
using Toolbar;

namespace TrackingStationUtils
{
	[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
	public class Toolbar : MonoBehaviour
	{
		IButton button;

		void Awake ()
		{
			addButton ();
		}

		void addButton () {
			button = ToolbarManager.Instance.add ("TrackingStationUtils", "main_menu");
			button.ToolTip = "Tracking Station Utils";
			button.Visibility = new GameScenesVisibility(GameScenes.TRACKSTATION);
			button.TexturePath = "TSUtils/Textures/tsutils_24";
			button.OnClick += (e) => onClick(button);
		}

		void onClick (IButton button)
		{
			if (button.Drawable == null) {
				createPopupMenu(button);
			} else {
				destroyPopupMenu(button);
			}
		}

		void createPopupMenu(IButton button) {
			bool vesselSelected = TrackingStationUtils.getSelectedVessel () != null;
			PopupMenuDrawable menu = new PopupMenuDrawable();
			IButton optRenameVessel = menu.AddOption("Rename vessel");
			optRenameVessel.Enabled = vesselSelected;
			optRenameVessel.OnClick += (e) => renameVessel();

			IButton optShowParts = menu.AddOption("Show parts");
			optShowParts.Enabled = vesselSelected;
			optShowParts.OnClick += (e) => showParts();

			IButton optShowInfo = menu.AddOption("Show resources");
			optShowInfo.Enabled = vesselSelected;
			optShowInfo.OnClick += (e) => showInfo();

			menu.OnAnyOptionClicked += () => destroyPopupMenu(button);
			button.Drawable = menu;
		}

		void showParts()
		{
			TrackingStationUtils.ShowParts ();
		}

		void showInfo ()
		{
			TrackingStationUtils.ShowVesselInfo ();
		}

		void renameVessel ()
		{
			TrackingStationUtils.RenameVessel(TrackingStationUtils.getSelectedVessel());
		}

		void destroyPopupMenu (IButton button)
		{
			((PopupMenuDrawable)button.Drawable).Destroy ();
			button.Drawable = null;
		}

		void OnDestroy()
		{
			if (button != null) {
				button.Destroy ();
			}
		}
	}
}
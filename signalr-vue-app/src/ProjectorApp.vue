<template>
  <div id="app-container">
    <div class="header-container">
      <Dropdown
          class="dropdown-container"
          label="Select Input:"
          v-model="state.selectedInput"
          :options="inputOptionsComputed"
          @change="handleDropdownChange"
      />
      <div class="toggles-container">
        <div class="toggle-container">
          <label class="toggle-label">
            VPN
          </label>
          <ToggleSwitch
              :isChecked="state.VPNConnected === projectorConstants.ToggleStatusGui.On"
              :disabled="state.VPNConnected === projectorConstants.ToggleStatusGui.Pending"
              @update:isChecked="handleVPNToggle"
              :class="{
                'power-toggle-disabled': state.VPNConnected === projectorConstants.ToggleStatusGui.Pending,
                'power-toggle-off': state.VPNConnected === projectorConstants.ToggleStatusGui.On,
                'power-toggle-on': state.VPNConnected === projectorConstants.ToggleStatusGui.Off
              }"
          />
        </div>
        <div class="toggle-container">
          <label class="toggle-label">
            Power
          </label>
          <ToggleSwitch
              :isChecked="state.ProjectorPoweredOn === projectorConstants.ToggleStatusGui.On"
              :disabled="state.ProjectorPoweredOn === projectorConstants.ToggleStatusGui.Pending"
              @update:isChecked="handlePowerToggle"
              :class="{
                'power-toggle-disabled': state.ProjectorPoweredOn === projectorConstants.ToggleStatusGui.Pending,
                'power-toggle-off': state.ProjectorPoweredOn === projectorConstants.ToggleStatusGui.On,
                'power-toggle-on': state.ProjectorPoweredOn === projectorConstants.ToggleStatusGui.Off
              }"
          />
        </div>        
      </div>
    </div>

    <!-- Tab Content Section -->
    <div class="content-container">
      <AndroidTVButtons
          v-if="selectedTab === 'adb'"
          :buttonDisabled="buttonDisabledWhenPoweredOffOrNotConnectedToAndroidTV"
          :handleClick="handleClickAndroidCommand"
      />
      <AndroidAppsTab
          v-if="selectedTab === 'apps'"
          :buttonDisabled="buttonDisabledWhenPoweredOffOrNotConnectedToAndroidTV"
          :handleClick="handleClickAndroidOpenAppCommand"
          :apps="availableApps"
      />
      <TvCommandsTab
          v-if="selectedTab === 'tv'"
          :handleClick="handleClickTVCommand"
      />
    </div>

    <!-- Tab Navigation Section -->
    <div class="tab-container">
      <div class="volume-row">
        <VolumeSlider 
          v-model="state.targetVolume"
          :isDisabled="buttonDisabledWhenPoweredOff"
          :onVolumeChange="handleVolumeChange"
        />
      </div>
      <div class="tab-row">
        <button
            v-for="tab in tabs"
            :key="tab.value"
            :class="{ active: selectedTab === tab.value }"
            @click="selectedTab = tab.value"
            class="tab-button"
        >
          <i :class="tab.icon"></i> <!-- Add icon based on tab value -->
          <span>{{ tab.label }}</span>
        </button>
      </div>
    </div>

  </div>
</template>

<script lang="ts" setup>
import { onMounted, onUnmounted, computed, ref } from "vue";
import { SignalRInstance } from "./SignalRServiceManager";
import * as projectorConstants from "./Constants/ProjectorConstants";
import Dropdown from "@/components/DropDown.vue";
import ToggleSwitch from "@/components/ToggleSwitch.vue";
import AndroidTVButtons from "@/Views/AndroidTVButtonLayout.vue";
import AndroidAppsTab from "@/Views/AndroidAppsButtonLayout.vue";
import TvCommandsTab from "@/Views/TVControlButtonLayout.vue";
import VolumeSlider from "@/components/VolumeSlider.vue";
import { useProjector } from "@/composables/useProjector";
import * as adbConstants from "@/Constants/AdbConstants";
import * as hdmiSwitchConstants from "@/Constants/HdmiSwitchConstants";

const handleVolumeChange = (newVolume: number) => {
  console.log(`Volume updated: ${newVolume}`);
  state.targetVolume = newVolume;
  SignalRInstance.sendProjectorVolumeSet(newVolume);
};

const {
  state,
  buttonDisabledWhenPoweredOffOrNotConnectedToAndroidTV,
  buttonDisabledWhenPoweredOff,
  handleDropdownChange,
  handleClickAndroidCommand,
  handleClickAndroidOpenAppCommand,
  handleClickTVCommand,
  handlePowerToggle,
  handleVPNToggle,
  handleProjectorConnectionStateChange,
  handleAndroidTVConnectionStateChange,
  handleHdmiInputQuery,
  handleProjectorQueryResponse,
  handleAndroidTVQueryResponse,
  handleGUIConnectionStateChange
} = useProjector();

const inputOptions = [
  { label: "Smart TV", value: hdmiSwitchConstants.Inputs.SmartTV },
  { label: "Cable TV", value: hdmiSwitchConstants.Inputs.CableTV },
  { label: "Nintendo Switch", value: hdmiSwitchConstants.Inputs.NintendoSwitch },
  { label: "Steam Link", value: hdmiSwitchConstants.Inputs.SteamLink },
  { label: "Open Hdmi", value: hdmiSwitchConstants.Inputs.OpenHdmi },
];

const inputOptionsComputed = computed(() => {
  return inputOptions.map((option) => ({
    ...option,
    disabled: state.ProjectorPoweredOn !== projectorConstants.ToggleStatusGui.On
  }));
});

const tabs = ref([
  { label: "SmartTV", value: "adb", icon: "fas fa-keyboard" },
  { label: "Apps", value: "apps", icon: "fas fa-tv" },
  { label: "TV Commands", value: "tv", icon: "fas fa-remote" }
]);

const availableApps = ref([
  { icon: "/assets/netflix-logo.png", alt: "Netflix", action: adbConstants.KeyCodes.Netflix },
  { icon: "/assets/youtube-logo.png", alt: "YouTube", action: adbConstants.KeyCodes.Youtube },
  { icon: "/assets/prime-video-logo.png", alt: "Prime Video", action: adbConstants.KeyCodes.AmazonPrime },
  { icon: "/assets/disney-logo.jpg", alt: "Disney+", action: adbConstants.KeyCodes.DisneyPlus },
  { icon: "/assets/spotify-logo.png", alt: "Spotify", action: adbConstants.KeyCodes.Spotify },
  { icon: "/assets/crunchyroll-logo.png", alt: "Crunchyroll", action: adbConstants.KeyCodes.Crunchyroll },
  { icon: "/assets/surfshark-logo.svg", alt: "Surfshark", action: adbConstants.KeyCodes.Surfshark },
]);

const selectedTab = ref("adb");

onMounted(async () => {
  await SignalRInstance.initialize(
      (isConnected) => { handleProjectorConnectionStateChange(isConnected); },
      (isConnected) => { handleAndroidTVConnectionStateChange(isConnected); },
      (response) => { handleHdmiInputQuery(response); },
      (response) => { handleAndroidTVQueryResponse(response.queryType, response.currentStatus); },
      (response) => { handleProjectorQueryResponse(response.queryType, response.currentStatus); },
      (connectionStatus) => { handleGUIConnectionStateChange(connectionStatus); }
  );
});

onMounted(() => {
  console.log("App mounted. Setting up tab focus listener.");
  window.addEventListener("focus", onTabFocused);
});

onUnmounted(() => {
  console.log("App unmounted. Removing tab focus listener.");
  window.removeEventListener("focus", onTabFocused);
});

const onTabFocused = () => {
  if (!SignalRInstance.isConnected()) {
    console.log("Tab regained focus. Reconnecting SignalR...");
    SignalRInstance.initialize(
        (isConnected) => { handleProjectorConnectionStateChange(isConnected); },
        (isConnected) => { handleAndroidTVConnectionStateChange(isConnected); },
        (response) => { handleHdmiInputQuery(response); },
        (response) => { handleAndroidTVQueryResponse(response.queryType, response.currentStatus); },
        (response) => { handleProjectorQueryResponse(response.queryType, response.currentStatus); },
        (connectionStatus) => { handleGUIConnectionStateChange(connectionStatus); }
    );
  } else {
    console.log("Tab regained focus. Querying initial backend statuses...");
    SignalRInstance.queryForInitialConnectionStatuses();
  }
};

</script>






<style>

html, body {
  margin: 0;
  padding: 0;
  width: 100%;
  box-sizing: border-box; /* Consistent box model */
  overflow-x: hidden; /* Prevent horizontal scrolling */
}

.app-container {
  height: 100%;
  width: 100%;
  display: flex;
  justify-content: center;
  justify-items: center;
  align-items: center;
}


.header-container {
  display: flex; /* Use Flexbox to handle layout */
  flex-direction: row; /* Lay elements out horizontally */
  align-items: center; /* Vertically align elements in the center */
  justify-content: space-between; /* Evenly distribute space between children */
  height: min(10vh, 10vw); /* Set height based on view height */
  padding: 0 2vw; /* Add horizontal padding for spacing */
  background-color: #f8f9fa; /* Optional background for header */
  border-bottom: 1px solid #ddd; /* Optional bottom border for styling */
  box-sizing: border-box; /* Ensure padding is included in height/width */
}

.dropdown-container {
  flex: 1; /* Allow the dropdown to take one portion of the space */
  display: flex; /* Align the inner contents */
  justify-content: center; /* Center dropdown horizontally */
  align-items: center; /* Center dropdown vertically */
  font-size: min(2.5vh, 2.5vw); /* Set font size relative to viewport height */
}

.dropdown-container select {
  height: min(8vh, 8vw);
  width: min(20vw, 20vh);
  font-size: min(3vw, 3vh)
}

.toggles-container {
  flex: 1; /* Allow toggles to take twice as much space as the dropdown */
  display: flex; /* Use Flexbox for alignment */
  justify-content: space-evenly; /* Distribute toggles evenly */
  align-items: center; /* Vertically align elements in the container */
  gap: 2vw; /* Add space between toggle groups */
}

/* Toggle container for label and switch alignment */
.toggle-container {
  display: flex; /* Set as a row layout */
  flex-direction: row; /* Ensure horizontal alignment */
  align-items: center; /* Vertically align the label and toggle */
  gap: 1vw; /* Add space between the label and the ToggleSwitch */
}

.toggle-label {
  font-size: min(2.5vh, 2.5vw); /* Set font size relative to viewport height */
}






.power-toggle-disabled .slider::before {
  background-color: #666; /* Grey knob when disabled */
}

.power-toggle-off .slider::before {
  background-color: #dc3545; /* Red knob when power is off */
}

.power-toggle-on .slider::before {
  background-color: #28a745; /* Green knob when power is on */
}

/* Default slider (background) */
.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #ccc; /* Default background (grey) */
  transition: 0.4s;
  border-radius: 34px; /* Makes slider background rounded */
}

/* Default toggle knob */
.slider::before {
  position: absolute;
  content: "";
  height: 26px;
  width: 26px;
  left: 4px;
  bottom: 4px;
  background-color: white; /* Change this to adjust default knob color */
  transition: 0.4s;
  border-radius: 50%; /* Makes the knob round */
}

/* Slider background when checked */
input:checked + .slider {
  background-color: #28a745; /* Green background for toggle ON */
}

/* Knob (toggle) color when checked */
input:checked + .slider::before {
  background-color: white; /* Change this if you want the toggle knob to change color */
}

/* Slider background when disabled */
input:disabled + .slider {
  background-color: #ccc; /* Grey out the slider when disabled */
  cursor: not-allowed;
  opacity: 0.6;
}

/* Disabled knob (toggle) */
input:disabled + .slider::before {
  background-color: #666; /* Grey out the knob when disabled */
}














.content-container {
  height: 75vh;
  max-width: 100vw;
  display: flex;
  justify-self: center;
  justify-content: center; /* Center horizontally within section */
  align-items: center; /* Center vertically within section */
  text-align: center; /* Center text for labels */
}










.tab-container {
  animation: slideUp 0.6s ease-out; /* Slide-up animation when the component loads */
  position: fixed; /* Keep the tabs pinned at the bottom */
  bottom: 0; /* Align to the bottom of the page */
  width: 100%; /* Full width */
  max-width: 100%;
  margin: 0;
  padding: 0;
  height: 15vh; /* Or dynamically calculated height */
  overflow: hidden;
  display: flex;               /* Use flexbox */
  flex-direction: column;      /* Stack rows vertically (column direction) */
  align-items: stretch;        /* Stretch rows to fill available space */
  justify-content: stretch; /* Equal space between buttons */
  background: #3a3a3a; /* Subtle gradient background */
  border-top: 1px solid #444; /* Optional: Top border as a divider */
  z-index: 10; /* Ensure this section stays above other content */
}

@keyframes slideUp {
  from {
    transform: translateY(100%); /* Start just below the viewport */
    opacity: 0; /* Start invisible */
  }
  to {
    transform: translateY(0); /* End in the default position */
    opacity: 1; /* Fully visible */
  }
}

/* Volume Row */
.volume-row {
  display: flex;                      /* Use flexbox for horizontal alignment */
  height: 9vh;
  justify-content: center;            /* Center the volume buttons horizontally */
  align-items: center;                /* Align buttons vertically */
  border-bottom: 1px solid #666;      /* Divider between rows */
  background-color: #383838;          /* Optional: background color for better visibility */
}

/* Tab Row */
.tab-row {
  display: flex;
  height: 6vh;
  justify-content: space-between;   /* Ensure buttons are spaced evenly */
  align-items: center;
  width: 100%;                      /* Full width of the row */
  gap: 10px;                        /* Space between buttons */
}

.tab-button {
  text-align: center; /* Center-align button text */
  color: #fff; /* White text color */
  background: linear-gradient(145deg, #1c1c1e, #333333); /* Button gradient */
  border: 1px solid #3d3d3d; /* Subtle border for separation */
  border-radius: 10px; /* Rounded corners */
  font-size: 3vh; /* Readable font size */
  font-weight: 600; /* Slightly bold text for better readability */
  cursor: pointer; /* Indicate clickability */
  transition: all 0.3s ease; /* Smooth animation on hover/focus */
  box-shadow: 0 3px 6px rgba(0, 0, 0, 0.2); /* Subtle shadow for depth */
  height: 5vh;
  width: 33vw;
}

.tab-button.active {
  background: linear-gradient(145deg, #007bff, #0056b3); /* Active button gradient */
  border-color: #003580; /* Stronger border for visibility */
  color: #fff; /* Ensure text is readable on the gradient */
  box-shadow: 0px 4px 10px rgba(0, 123, 255, 0.5); /* Glow effect for active state */
}

.tab-button:hover {
  background: linear-gradient(145deg, #3d3d3d, #555); /* Change on hover */
  box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.4); /* Slightly larger shadow on hover */
  transform: scale(1.05); /* Add a slight scale effect for interaction feedback */
}

.tab-button:active {
  transform: scale(0.95); /* Subtle shrink effect when clicking */
  background: linear-gradient(145deg, #222, #444); /* Darker color for interaction feedback */
}














/* Responsive Font Size for Smaller Screens */
@media (max-width: 768px) {
  .tab-button {
    font-size: 14px; /* Reduce font size slightly on smaller devices */
  }
}

</style>
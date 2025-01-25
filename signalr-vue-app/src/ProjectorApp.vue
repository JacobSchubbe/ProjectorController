<template>
  <div id="app">
    <div class="header-container">
      <!-- Left Section (Dropdown Menu) -->
      <div class="header-item">
        <Dropdown
            label="Select Input:"
            v-model="state.selectedInput"
            :options="inputOptions"
            @change="handleDropdownChange"
        />
      </div>
      
      <!-- Right Section (Projector Power Toggle) -->
      <div class="header-section">
        <div class="toggle-container">
          <label class="toggle-label">
            Projector Power
          </label>
          <ToggleSwitch
              :isChecked="state.ProjectorPoweredOn === projectorConstants.PowerStatusGui.On"
              :disabled="state.ProjectorPoweredOn === projectorConstants.PowerStatusGui.Pending"
              @update:isChecked="handlePowerToggle"
              :class="{
                'power-toggle-disabled': state.ProjectorPoweredOn === projectorConstants.PowerStatusGui.Pending,
                'power-toggle-off': state.ProjectorPoweredOn === projectorConstants.PowerStatusGui.On,
                'power-toggle-on': state.ProjectorPoweredOn === projectorConstants.PowerStatusGui.Off
              }"
          />
        </div>
      </div>
    </div>

    <!-- Tab Content Section -->
    <div class="tab-content">
      <AdbKeyCodesTab
          v-if="selectedTab === 'adb'"
          :buttonDisabled="buttonDisabledWhenPowerOff"
          :handleClick="handleClickAndroidCommand"
          :availableHeight="availableHeight"
      />
      <AndroidAppsTab
          v-if="selectedTab === 'apps'"
          :disabled="buttonDisabledWhenPowerOff"
          :buttonDisabled="buttonDisabledWhenPowerOff"
          :handleClick="handleClickAndroidOpenAppCommand"
          :apps="availableApps"
          :availableHeight="availableHeight"
      />
      <TvCommandsTab
          v-if="selectedTab === 'tv'"
          :handleClick="handleClickTVCommand"
          :availableHeight="availableHeight"
      />
    </div>

    <!-- Tab Navigation Section -->
    <div class="tab-container">
      <div class="volume-row">
        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickProjectorCommands(projectorConstants.ProjectorCommands.KeyControlVolumeDown)"
            class="volume-button"
        >
          Volume<br/><br/>-
        </ControlButton>
        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickProjectorCommands(projectorConstants.ProjectorCommands.KeyControlVolumeUp)"
            class="volume-button"
        >
          Volume<br/><br/>+
        </ControlButton>
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
import { onMounted, onUnmounted, ref } from "vue";
import { SignalRInstance } from "./SignalRServiceManager";
import * as projectorConstants from "./Constants/ProjectorConstants";
import Dropdown from "@/components/DropDown.vue";
import ToggleSwitch from "@/components/ToggleSwitch.vue";
import ControlButton from "@/components/ControlButton.vue";
import AdbKeyCodesTab from "@/Views/AndroidTVButtonLayout.vue";
import AndroidAppsTab from "@/Views/AndroidAppsButtonLayout.vue";
import TvCommandsTab from "@/Views/TVControlButtonLayout.vue";
import { useProjector } from "@/composables/useProjector";
import * as adbConstants from "@/Constants/AdbConstants";

// Reactive variable to store available height
const availableHeight = ref(0);

const calculateAvailableHeight = () => {
  // Query the size of the top header and bottom tab menu
  const header = document.querySelector(".header-container") as HTMLElement;
  const tabContainer = document.querySelector(".tab-container") as HTMLElement;

  // Calculate space between the menus
  const totalHeight = window.innerHeight;
  const headerHeight = header?.offsetHeight || 0;
  const tabHeight = tabContainer?.offsetHeight || 0;

  // Assign available height
  availableHeight.value = totalHeight - headerHeight - tabHeight;
};

const {
  state,
  buttonDisabledWhenPowerOff,
  handleDropdownChange,
  handleClickProjectorCommands,
  handleClickAndroidCommand,
  handleClickAndroidOpenAppCommand,
  handleClickTVCommand,
  handlePowerToggle,
  handleProjectorConnectionStateChange,
  handleAndroidTVConnectionStateChange,
  handleProjectorQueryResponse,
  handleGUIConnectionStateChange
} = useProjector();

const inputOptions = [
  { label: "TV/Switch", value: projectorConstants.ProjectorCommands.SystemControlSourceHDMI1 },
  { label: "SmartTV", value: projectorConstants.ProjectorCommands.SystemControlSourceHDMI3 },
];

const tabs = ref([
  { label: "SmartTV", value: "adb", icon: "fas fa-keyboard" },
  { label: "Apps", value: "apps", icon: "fas fa-tv" },
  { label: "TV Commands", value: "tv", icon: "fas fa-remote" }
]);

const availableApps = ref([
  { icon: "/assets/netflix-logo.png", alt: "Netflix", action: adbConstants.KeyCodes.Netflix },
  { icon: "/assets/youtube-logo.png", alt: "YouTube", action: adbConstants.KeyCodes.Youtube },
  { icon: "/assets/prime-video-logo.png", alt: "Prime Video", action: adbConstants.KeyCodes.AmazonPrime },
  { icon: "/assets/disney-logo.jpg", alt: "Disney+", action: adbConstants.KeyCodes.DisneyPlus }
]);

const selectedTab = ref("adb");

onMounted(async () => {
  calculateAvailableHeight();
  window.addEventListener("resize", calculateAvailableHeight);
  window.addEventListener("orientationchange", calculateAvailableHeight); // Handle rotation on mobile
});

onMounted(async () => {
  await SignalRInstance.initialize(
      (isConnected) => { handleProjectorConnectionStateChange(isConnected); },
      (isConnected) => { handleAndroidTVConnectionStateChange(isConnected); },
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

onUnmounted(() => {
  window.removeEventListener("resize", calculateAvailableHeight);
  window.removeEventListener("orientationchange", calculateAvailableHeight);
});

const onTabFocused = () => {
  if (!SignalRInstance.isConnected()) {
    console.log("Tab regained focus. Reconnecting SignalR...");
    SignalRInstance.initialize(
        (isConnected) => handleProjectorConnectionStateChange(isConnected),
        (isConnected) => handleAndroidTVConnectionStateChange(isConnected),
        (response) => handleProjectorQueryResponse(response.queryType, response.currentStatus),
        (connectionStatus) => handleGUIConnectionStateChange(connectionStatus)
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
}

button {
  margin: 10px;
  padding: 10px;
}

select {
  margin: 10px;
  padding: 5px;
}

.projector-controls {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px; /* Spacing between rows */
}

.control-row {
  display: flex;
  justify-content: center;
  gap: 10px; /* Spacing between buttons */
}

.control-button {
  background-color: #007bff; /* Primary button color */
  color: white;
  border: none;
  border-radius: 5px;
  width: 80px; /* Fixed width */
  height: 80px; /* Fixed height */
  font-size: 14px; /* Reduce font size to prevent overflow if content is long */
  cursor: pointer;
  text-align: center;
  vertical-align: middle;
  display: flex; /* Flexbox for centering content */
  justify-content: center; /* Horizontally center content */
  align-items: center; /* Vertically center content */
  transition: background-color 0.3s ease, transform 0.2s;
  white-space: normal; /* Allow text to wrap inside the button */
  overflow: hidden; /* Hide any overflowing content */
  text-overflow: ellipsis; /* Add ellipsis if text overflows */
}

.control-button:disabled {
  background-color: #ccc; /* Grey background */
  color: #666;           /* Greyed-out text */
  cursor: not-allowed;   /* Change cursor to indicate it's not clickable */
  opacity: 0.6;          /* Dim the button */
}

.control-button:hover {
  background-color: #0056b3; /* Darker shade on hover */
}

.control-button:active {
  transform: scale(0.95); /* Button press effect */
}

.enter-button {
  background-color: #28a745; /* Distinct color for the Enter button */
}

.enter-button:hover {
  background-color: #1e7e34; /* Darker shade on hover for Enter button */
}

.media-button {
  background-color: transparent; /* Make the button background fully transparent */
  border: none; /* Remove borders for a cleaner visual */
  padding: 0; /* Remove extra spacing around the image */
  display: flex; /* Use flexbox for proper content alignment */
  justify-content: center;
  align-items: center;
  width: 80px; /* Button should remain square */
  height: 80px; /* Fixed button height (equal to width) */
  cursor: pointer;
  overflow: hidden; /* Clip any overflow from the image */
}

.media-button:disabled {
  opacity: 0.6;
  pointer-events: none; /* Disable pointer interactions when disabled */
}

.media-icon {
  width: 100%; /* Ensure image completely fills the button */
  height: 100%; /* Scale image to fill height */
  object-fit: cover; /* Make sure image covers the entire button evenly */
  pointer-events: none; /* Ensure clicking on the image still triggers the button */
}

.header-container {
  display: flex; /* Create flex container for horizontal row */
  width: 100%; /* Full width of screen */
  background-color: #f8f9fa; /* Optional: Light background */
  border-bottom: 1px solid #ddd; /* Optional: Divider */
  padding: 10px 0; /* Add some padding above and below header */
}

/* Each section (1/3 width) */
.header-item {
  flex: 1; /* Make each section take 1/3 of the container */
  display: flex; /* Use flexbox for centering */
  justify-content: center; /* Center content horizontally in each section */
  align-items: center; /* Center content vertically in each section */
  text-align: center; /* Center text within the section */
}

.header-section {
  flex: 1; /* Each section takes 1/3 of the total width */
  display: flex; /* Flexbox for aligning child elements */
  justify-content: center; /* Center horizontally within section */
  align-items: center; /* Center vertically within section */
  text-align: center; /* Center text for labels */
  padding: 0 10px; /* Optional: Horizontal padding */
}

/* Toggle container for label and switch alignment */
.toggle-container {
  display: flex; /* Set as a row layout */
  flex-direction: row; /* Ensure horizontal alignment */
  align-items: center; /* Vertically align the label and toggle */
  gap: 10px; /* Add space between the label and the ToggleSwitch */
}

/* Align text within the toggle container */
.centered-text {
  text-align: center; /* Center text */
  margin-bottom: 8px; /* Add spacing between label and toggle switch */
}

label {
  font-size: 14px; /* Optional: Adjust label font size */
  margin: 0;
}

.right-aligned {
  display: flex;
  flex-direction: column; /* Optional: stack text and toggle vertically */
  align-items: flex-end; /* Align items to the right */
  text-align: right; /* Align text within the container to the right */
  white-space: nowrap; /* Prevent text from wrapping unless desired */
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


.tab-container {
  animation: slideUp 0.6s ease-out; /* Slide-up animation when the component loads */
  position: fixed; /* Keep the tabs pinned at the bottom */
  bottom: 0; /* Align to the bottom of the page */
  width: 100%; /* Full width */
  display: flex;               /* Use flexbox */
  flex-direction: column;      /* Stack rows vertically (column direction) */
  align-items: stretch;        /* Stretch rows to fill available space */
  justify-content: space-around; /* Equal space between buttons */
  background: linear-gradient(90deg, #3a3a3a, #262626); /* Subtle gradient background */
  border-top: 1px solid #444; /* Optional: Top border as a divider */
  padding: 10px 0; /* Padding for better spacing */
  z-index: 10; /* Ensure this section stays above other content */
}

.tab-button {
  flex: 1; /* Equal size for every button */
  text-align: center; /* Center-align button text */
  color: #fff; /* White text color */
  background: linear-gradient(145deg, #1c1c1e, #333333); /* Button gradient */
  border: 1px solid #3d3d3d; /* Subtle border for separation */
  border-radius: 10px; /* Rounded corners */
  font-size: 16px; /* Readable font size */
  font-weight: 600; /* Slightly bold text for better readability */
  padding: 10px 0; /* Padding for the button text */
  cursor: pointer; /* Indicate clickability */
  transition: all 0.3s ease; /* Smooth animation on hover/focus */
  box-shadow: 0 3px 6px rgba(0, 0, 0, 0.2); /* Subtle shadow for depth */
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

/* Volume Row */
.volume-row {
  display: flex;                      /* Use flexbox for horizontal alignment */
  justify-content: center;            /* Center the volume buttons horizontally */
  align-items: center;                /* Align buttons vertically */
  gap: 15px;                          /* Space between Volume + and Volume - */
  padding: 10px 0;                    /* Padding for the row */
  border-bottom: 1px solid #666;      /* Divider between rows */
  background-color: #383838;          /* Optional: background color for better visibility */
}

/* Tab Row */
.tab-row {
  display: flex;
  justify-content: space-between;   /* Ensure buttons are spaced evenly */
  align-items: center;
  width: 100%;                      /* Full width of the row */
  gap: 10px;                        /* Space between buttons */
}

/* Volume Button Styles */
.volume-button {
  background-color: #1c79cb; /* Match the active tab color */
  color: white;             /* White text for contrast */
  border: none;
  padding: 10px 20px;                 /* Add padding for size */
  border-radius: 8px;                 /* Rounded corners */
  font-size: 14px;                    /* Medium text size */
  font-weight: bold;                  /* Make text bold */
  cursor: pointer;
  transition: background-color 0.3s ease, transform 0.2s ease; /* Smooth hover effect */
}

/* Volume Button - Hover Effect */
.volume-button:hover {
  background-color: #0056b3; /* Slightly darker shade on hover, similar to active tab hover */
  transform: translateY(-2px); /* Lift the button slightly */
}

/* Responsive Font Size for Smaller Screens */
@media (max-width: 768px) {
  .tab-button {
    font-size: 14px; /* Reduce font size slightly on smaller devices */
  }
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



</style>
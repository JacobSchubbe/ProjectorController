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

      <!-- Center Section (GUI Status) -->
      <div class="header-item">
        <label>GUI Connected: {{ state.GUIConnected }}</label>
      </div>

      <!-- Right Section (Projector Power Toggle) -->
      <div class="header-section">
        <div class="toggle-container">
          <label class="toggle-label">
            Projector Power
          </label>
          <ToggleSwitch
              :isChecked="state.ProjectorPoweredOn"
              :disabled="buttonDisabledPowerButton"
              @update:isChecked="handlePowerToggle"
          />
        </div>
      </div>
    </div>
    <div class="projector-controls">
      <div class="control-row">

        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_BACK)"
        >
          Back
        </ControlButton>
        
        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_DPAD_UP)"
        >
          Up
        </ControlButton>
        
        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_HOME)"
        >
          Home
        </ControlButton>
      </div>
      
      <div class="control-row">
        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_DPAD_LEFT)"
        >
          Left
        </ControlButton>

        <ControlButton
            styleClass="control-button enter-button"
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_ENTER)"
        >
          Enter
        </ControlButton>

        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_DPAD_RIGHT)"
        >
          Right
        </ControlButton>
      </div>
      
      <div class="control-row">
        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_DPAD_DOWN)"
        >
          Down
        </ControlButton>
      </div>
      
      <div class="control-row">
        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickProjectorCommands(projectorConstants.ProjectorCommands.KeyControlVolumeDown)"
        >
          Volume<br/><br/>-
        </ControlButton>
        <ControlButton
            :disabled="buttonDisabledWhenPowerOff"
            :onClick="() => handleClickProjectorCommands(projectorConstants.ProjectorCommands.KeyControlVolumeUp)"
        >
          Volume<br/><br/>+
        </ControlButton>
      </div>

      <!-- Media Controls -->
      <div class="control-row">
        <MediaButton
            icon="/assets/netflix-logo.png"
            class="control-button media-button"
            alt="Netflix"
            :onClick="() => handleClickAndroidOpenAppCommand(adbConstants.KeyCodes.Netflix)"
            :disabled="buttonDisabledWhenPowerOff"
        />
        <MediaButton
            icon="/assets/youtube-logo.png"
            class="control-button media-button"
            alt="YouTube"
            :onClick="() => handleClickAndroidOpenAppCommand(adbConstants.KeyCodes.Youtube)"
            :disabled="buttonDisabledWhenPowerOff"
        />
      </div>
      <div class="control-row">
        <MediaButton
            icon="/assets/prime-video-logo.png"
            class="control-button media-button"
            alt="PrimeVideo"
            :onClick="() => handleClickAndroidOpenAppCommand(adbConstants.KeyCodes.AmazonPrime)"
            :disabled="buttonDisabledWhenPowerOff"
        />
        <MediaButton
            icon="/assets/disney-logo.jpg"
            class="control-button media-button"
            alt="DisneyPlus"
            :onClick="() => handleClickAndroidOpenAppCommand(adbConstants.KeyCodes.DisneyPlus)"
            :disabled="buttonDisabledWhenPowerOff"
        />
      </div>
      <div class="control-row">
        <ControlButton
            :onClick="() => handleClickTVCommand(tvConstants.IRCommands.KEY_POWER)"
        >
          TV Power
        </ControlButton>
      </div>
      
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, onUnmounted } from "vue";
import { SignalRInstance } from "./SignalRServiceManager";
import * as adbConstants from "./Constants/AdbConstants";
import * as projectorConstants from "./Constants/ProjectorConstants";
import * as tvConstants from "./Constants/TVConstants";
import Dropdown from "@/components/DropDown.vue";
import ControlButton from "@/components/ControlButton.vue";
import MediaButton from "@/components/MediaButton.vue";
import { useProjector } from "@/composables/useProjector";
import ToggleSwitch from "@/components/ToggleSwitch.vue";

const {
  state,
  buttonDisabledPowerButton,
  buttonDisabledWhenPowerOff,
  handleDropdownChange,
  handleClickProjectorCommands,
  handleClickTVCommand,
  handleClickAndroidCommand,
  handleClickAndroidOpenAppCommand,
  handleProjectorConnectionStateChange,
  handleAndroidTVConnectionStateChange,
  handleProjectorQueryResponse,
  handleGUIConnectionStateChange,
  handlePowerToggle
} = useProjector();

const inputOptions = [
  { label: "TV/Switch", value: projectorConstants.ProjectorCommands.SystemControlSourceHDMI1 },
  { label: "SmartTV", value: projectorConstants.ProjectorCommands.SystemControlSourceHDMI3 },
];

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

</style>
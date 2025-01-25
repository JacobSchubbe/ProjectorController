<template>
  <div class="grid-containers">
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_POWER)"
        class="red-button"
      >
        Power
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_SUBTITLE)"
      >
        Subtitle
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_MUTE)"
      >
        Mute
      </ControlButton>
    </div>
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_EPG)"
      >
        Episode Guide
      </ControlButton>
      <ControlButton
      >
        -
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_PAGEUP)"
      >
        Page+
      </ControlButton>
    </div>
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_AUDIO)"
      >
        Audio
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_FILE)"
      >
        FileList
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_PAGEDOWN)"
      >
        Page-
      </ControlButton>
    </div>
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_MENU)"
      >
        Menu
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_UP)"
        class="green-button"
      >
        Up
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_EXIT)"
      >
        Exit
      </ControlButton>
    </div>
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_LEFT)"
        class="green-button"
      >
        Left
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_OK)"
        class="green-button"
      >
        OK
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_RIGHT)"
        class="green-button"
      >
        Right
      </ControlButton>
    </div>
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_FAVORITES)"
      >
        Favorites
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_DOWN)"
        class="green-button"
      >
        Down
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_TV)"
      >
        TV/Radio
      </ControlButton>
    </div>
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_1)"
      >
        1
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_2)"
      >
        2
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_3)"
      >
        3
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_BACK)"
      >
        Recall
      </ControlButton>
    </div>
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_4)"
      >
        4
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_5)"
      >
        5
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_6)"
      >
        6
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_INFO)"
      >
        Info
      </ControlButton>
    </div>
    <div class="control-row" :style="{ height: `${buttonSize}px`, '--buttonSize': `${buttonSize}px` }">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_7)"
      >
        7
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_8)"
      >
        8
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_9)"
      >
        9
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_NUMERIC_0)"
      >
        0
      </ControlButton>
    </div>
  </div>
</template>

<script lang="ts" setup>
import * as tvConstants from "@/Constants/TVConstants";
import ControlButton from "@/components/ControlButton.vue";
import { defineProps, computed } from "vue";

const props = defineProps<{
  handleClick: () => void;
  availableHeight: number;
}>();

// Dynamically calculate button size
const buttonSize = computed(() => {
  const numRows = 9; // Number of button rows (change per your layout)
  const marginsPerRow = 2; // Total margin per row
  return (props.availableHeight - marginsPerRow * numRows) / numRows;
});

</script>

<style scoped>

.grid-container {
  display: grid;
  height: calc(100vh - 60px); /* Full height minus the bottom tab-container */
  grid-template-columns: repeat(3, 1fr); /* 3 buttons per row */
  grid-template-rows: auto; /* Rows adjust automatically based on content */
  gap: 10px; /* Spacing between buttons */
  padding: 10px; /* Optional padding for spacing */
  box-sizing: border-box; /* Make padding included in height/width */
  overflow: hidden; /* Prevent overflow outside the container */
}

button,
.ControlButton { /* If Buttons use a custom component */
  width: 100%; /* Buttons should fill their grid cell */
  height: 100%; /* Height matches the row height */
  font-size: calc(0.3 * var(--buttonSize)); /* Scale font size relative to button size */
  display: flex; /* Flexbox to align content */
  justify-content: center; /* Center-align text horizontally */
  align-items: center; /* Center-align text vertically */
  border: 1px solid #ccc;
  background-color: #007bff;
  color: white;
  border-radius: 5px; /* Optional rounded corners */
  box-sizing: border-box;
}

@media (max-width: 768px) {
  .grid-container {
    grid-template-columns: repeat(2, 1fr); /* Two buttons per row */
  }
}

@media (max-width: 480px) {
  .grid-container {
    grid-template-columns: 1fr; /* One button per row */
  }
}

.red-button {
  background-color: red;
  color: white; /* Ensure the text is readable */
}

.green-button {
  background-color: green;
  color: white; /* Ensure the text is readable */
}

</style>
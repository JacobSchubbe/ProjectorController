<template>
  <div class="grid-containers">
    <div class="control-row">
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
    <div class="control-row">
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
    <div class="control-row">
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
    <div class="control-row">
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
    <div class="control-row">
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
    <div class="control-row">
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
    <div class="control-row">
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
    <div class="control-row">
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
    <div class="control-row">
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
import { defineProps } from "vue";

defineProps<{
  handleClick: () => void;
}>();

</script>

<style scoped>

.control-row {
  display: flex; /* Ensure buttons in the row are horizontally aligned */
  height: 8vh;
  justify-content: space-between; /* Space between buttons */
  align-items: center; /* Align buttons vertically at the center */
}

.grid-container {
  display: grid;
  height: 100%;
  width: 100%;
  grid-template-columns: repeat(3, 1fr); /* 3 buttons per row */
  grid-template-rows: auto; /* Rows adjust automatically based on content */
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
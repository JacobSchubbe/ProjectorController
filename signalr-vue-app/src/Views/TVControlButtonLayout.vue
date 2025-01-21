<template>
  <div>
    <div class="control-row">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_POWER)"
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
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_MENU)"
      >
        Menu
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_UP)"
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
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_LEFT)"
      >
        Left
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_OK)"
      >
        OK
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_RIGHT)"
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

defineProps({
  handleClick: Function // Function to handle button clicks
});
</script>
<template>
  <div class="dpad-container">
    <!-- Circular D-Pad -->
    <div
        class="dpad-touch-area"
        :class="{ 'disabled': props.buttonDisabled }"
        :disabled="props.buttonDisabled"
        @touchstart="handleTouchStart"
        @touchmove="handleTouchMove"
        @touchend="handleTouchEnd"
        @contextmenu.prevent>
    </div>
  </div>
</template>

<script lang="ts" setup>
import * as adbConstants from "@/Constants/AdbConstants";
import { ref } from "vue";
import { defineProps } from "vue";

const props = defineProps<{
  buttonDisabled: Boolean; // Passed down from the parent component
  handleClick: Function; // Function signature with args
}>();

// State variables
const startX = ref(0); // Starting X-coordinate of the touch
const startY = ref(0); // Starting Y-coordinate of the touch
const touchStartTime = ref(0); // Timestamp when the touch started
const activeDirection = ref<string | null>(null); // Active swipe direction
const enterTriggered = ref(false); // Flag to prevent additional actions after long press

// Timers
let longPressTimer: number | null = null; // Timer for long press actions
let intervalId: number | null = null; // Timer for repeated swipe commands

// Thresholds
const TAP_THRESHOLD_PX = 15; // Distance (in px) for detecting tap/hold
const LONG_PRESS_DELAY = 1000; // Time (in ms) to detect long press
const SWIPE_REPEAT_DELAY = 500; // Frequency for directional repeated commands

// Utility function to calculate distance between touch points
const getTouchDistance = (touchX: number, touchY: number) => {
  const dx = touchX - startX.value;
  const dy = touchY - startY.value;
  return Math.sqrt(dx * dx + dy * dy); // Pythagorean distance
};

// Trigger a key action
const handleTriggerClick = (keyCode: adbConstants.KeyCodes, isLongPress: boolean) => {
  console.log(`Key action triggered: ${keyCode}`); // Replace with your actual key press logic
  props.handleClick?.(keyCode, isLongPress.toString());
};

// Start a repeated command for swiping
const startSendingCommand = (keyCode: adbConstants.KeyCodes) => {
  handleTriggerClick(keyCode, false); // Trigger action immediately
  intervalId = setInterval(() => {
    handleTriggerClick(keyCode, false); // Repeatedly trigger action
  }, SWIPE_REPEAT_DELAY);
};

// Stop repeated commands
const stopSendingCommand = () => {
  if (intervalId !== null) {
    clearInterval(intervalId);
    intervalId = null;
  }
};

// Handle the start of a touch
const handleTouchStart = (event: TouchEvent) => {
  if (props.buttonDisabled)
    return;
  
  // Record the start position and timestamp
  startX.value = event.touches[0].clientX;
  startY.value = event.touches[0].clientY;
  touchStartTime.value = performance.now(); // Record the touch start time

  // Reset state
  enterTriggered.value = false; // Allow new Enter actions
  activeDirection.value = null; // Reset swipe direction

  // Start the long press timer
  longPressTimer = setTimeout(() => {
    // Trigger the long press action immediately if the timer exceeds
    handleTriggerClick(adbConstants.KeyCodes.KEYCODE_ENTER, true); // Long press Enter
    console.log("Long Press Enter triggered");
    enterTriggered.value = true; // Mark Enter as completed
    longPressTimer = null; // Clear the timer since action was triggered
  }, LONG_PRESS_DELAY);
};

// Handle touch movement for swiping
const handleTouchMove = (event: TouchEvent) => {
  if (props.buttonDisabled)
    return;
  
  const moveX = event.touches[0].clientX;
  const moveY = event.touches[0].clientY;

  // If the user has already executed a long press, don't process further
  if (enterTriggered.value) {
    return;
  }

  const distance = getTouchDistance(moveX, moveY); // Calculate distance from start point

  if (distance > TAP_THRESHOLD_PX) {
    // If movement exceeds the threshold, cancel the long press timer
    if (longPressTimer !== null) {
      clearTimeout(longPressTimer);
      longPressTimer = null;
    }

    // Detect swipe direction
    let newDirection: string | null = null;
    let keyCode: number | null = null;

    const dx = moveX - startX.value; // Horizontal distance
    const dy = moveY - startY.value; // Vertical distance

    if (Math.abs(dx) > Math.abs(dy)) {
      // Horizontal swipe
      newDirection = dx > 0 ? "right" : "left";
      keyCode = dx > 0
          ? adbConstants.KeyCodes.KEYCODE_DPAD_RIGHT
          : adbConstants.KeyCodes.KEYCODE_DPAD_LEFT;
    } else {
      // Vertical swipe
      newDirection = dy > 0 ? "down" : "up";
      keyCode = dy > 0
          ? adbConstants.KeyCodes.KEYCODE_DPAD_DOWN
          : adbConstants.KeyCodes.KEYCODE_DPAD_UP;
    }

    // If direction changes, update and start swipe actions
    if (newDirection && activeDirection.value !== newDirection) {
      activeDirection.value = newDirection; // Update direction
      stopSendingCommand(); // Stop previous direction commands
      if (keyCode) startSendingCommand(keyCode); // Start new swipe direction
    }
  }
};

// Handle the end of a touch
const handleTouchEnd = (event: TouchEvent) => {
  if (props.buttonDisabled)
    return;
  
  // Stop any ongoing swipe commands
  stopSendingCommand();

  // Ignore further actions if the long press was already triggered
  if (enterTriggered.value) {
    console.log("Action already triggered by long press. Ignoring touch end.");
    return;
  }

  // Measure how long the touch lasted
  const touchEndTime = performance.now();
  const elapsedTime = touchEndTime - touchStartTime.value;

  // Calculate the final touch distance
  const endX = event.changedTouches[0].clientX;
  const endY = event.changedTouches[0].clientY;
  const distance = getTouchDistance(endX, endY);

  // Determine the action based on distance and time
  if (distance <= TAP_THRESHOLD_PX) {
    // If within tap threshold
    if (elapsedTime <= LONG_PRESS_DELAY) {
      // Short press detected
      handleTriggerClick(adbConstants.KeyCodes.KEYCODE_ENTER, false); // Short press Enter
      console.log("Short Tap Enter triggered");
    } else {
      // Long press should already have been triggered by the timer
      console.log("Long press timer already triggered; no additional action.");
    }
  } else {
    // If moved outside the threshold, clear state
    console.log("Swipe gesture detected; no further action on touch end.");
  }

  // Reset all state variables
  activeDirection.value = null;
  if (longPressTimer !== null) {
    clearTimeout(longPressTimer);
    longPressTimer = null;
  }
  enterTriggered.value = false;
};
</script>

<style scoped>

.dpad-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%; /* Full-width container */
  height: 40vh; /* Adjust height as needed */
  user-select: none;
  -webkit-user-select: none; /* Safari */
  -ms-user-select: none; /* IE 10+ */
}

.dpad-touch-area {
  position: relative;
  width: 35vh;
  height: 35vh;
  background: linear-gradient(145deg, #3a3a3a, #505050); /* Dark gradient matching other components */
  border: 3px solid #444; /* Border matching other components */
  border-radius: 50%; /* Makes it circular */
  touch-action: none; /* Disable default touch behavior */
  display: flex;
  justify-content: center;
  align-items: center;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.2); /* Subtle shadow for depth */
}

.dpad-touch-area.disabled {
  background-color: #ccc; /* Grey background */
  color: #666;           /* Greyed-out text */
  cursor: not-allowed;   /* Change cursor to indicate it's not clickable */
  opacity: 0.6;          /* Add a dim effect */
  pointer-events: none;  /* Completely disable events */
}

</style>
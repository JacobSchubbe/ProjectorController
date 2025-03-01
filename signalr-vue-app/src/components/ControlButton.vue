<template>
  <button
      :class="[styleClass]"
      :disabled="disabled"
      @pointerdown="startPress"
      @pointerup="endPress.bind(this, false)"
      @pointerleave="endPress.bind(this, true)"
      @contextmenu.prevent
  >
    <slot></slot>
  </button>
</template>

<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  name: "ControlButton", 
  props: {
    onClick: {
      // Correctly type the onClick prop
      type: Function,
      required: true,
    },
    disabled: {
      type: Boolean,
      default: false,
    },
    styleClass: {
      type: String,
      default: "control-button",
    },
  },
  data() {
    return {
      pressTimer: null as number | null, // Timer for detecting long presses
      isLongPress: false, // Flag for whether it's a long press
      pressStartTime: 0, // Track timestamp of the mousedown event
    };
  },
  mounted() {
    document.addEventListener("pointerup", this.globalEndPress);
  },
  beforeUnmount() {
    document.removeEventListener("pointerup", this.globalEndPress);
  },
  methods: {
    startPress(): void {
      // Prevent any press actions if the button is disabled
      if (this.disabled) {
        return;
      }

      // Record the start time of the press
      this.pressStartTime = Date.now();

      // Reset long press flag
      this.isLongPress = false;

      // Start timer for long press detection
      this.pressTimer = window.setTimeout(() => {
        this.isLongPress = true; // Mark as a long press
        this.onClick('true'); // Trigger long press handler
        this.pressTimer = null; // Reset timer
      }, 800); // Long press threshold (800ms)
    },

    endPress(isCancelled: boolean): void {
      // Prevent any press actions if the button is disabled
      if (this.disabled) {
        return;
      }

      // Clear the timer immediately if it exists
      if (this.pressTimer) {
        window.clearTimeout(this.pressTimer);
        this.pressTimer = null;
      }

      // Calculate the duration of the press
      const pressDuration = Date.now() - this.pressStartTime;

      // If it's not cancelled, and the press duration was below 800ms, trigger short press
      if (!isCancelled && pressDuration < 800 && !this.isLongPress) {
        this.onClick('false'); // Trigger short press handler
      }

      // Reset `isLongPress` flag after handling
      this.isLongPress = false;
    },

    cancelPress(): void {
      // Clear the timer immediately
      if (this.pressTimer) {
        window.clearTimeout(this.pressTimer);
        this.pressTimer = null;
      }

      // Reset long press flag
      this.isLongPress = false;
    },

    globalEndPress() {
      // If global mouseup is detected, call endPress
      this.endPress(false);
    },
  },
});
</script>

<style scoped>
button {
  margin: 10px;
  padding: 10px;
  /* Prevent text selection */
  user-select: none;
  -webkit-user-select: none; /* Safari */
  -ms-user-select: none; /* IE 10+ */
}

.control-button {
  background-color: #007bff; /* Primary button color */
  color: white;
  border: none;
  border-radius: 5px;
  height: 100%;
  width: 100%;
  font-size: 2vh; /* Reduce font size to prevent overflow if content is long */
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

.control-button:active {
  transform: scale(0.95); /* Button press effect */
}

</style>
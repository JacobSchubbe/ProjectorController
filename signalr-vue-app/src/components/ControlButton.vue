<template>
  <button
      :class="[styleClass]"
      :disabled="disabled"
      @pointerdown="startPress"
      @pointerup="endPress.bind(this, false)"
      @pointerleave="endPress.bind(this, true)"
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
      // Record the start time of the press
      this.pressStartTime = Date.now();

      // Reset long press flag
      this.isLongPress = false;

      // Start timer for long press detection
      this.pressTimer = window.setTimeout(() => {
        this.isLongPress = true; // Mark as a long press
        this.onClick(true); // Trigger long press handler
        this.pressTimer = null; // Reset timer
      }, 800); // Long press threshold (800ms)
    },

    endPress(isCancelled: boolean): void {
      // Clear the timer immediately if it exists
      if (this.pressTimer) {
        window.clearTimeout(this.pressTimer);
        this.pressTimer = null;
      }

      // Calculate the duration of the press
      const pressDuration = Date.now() - this.pressStartTime;

      // If it's not cancelled, and the press duration was below 800ms, trigger short press
      if (!isCancelled && pressDuration < 800 && !this.isLongPress) {
        this.onClick(false); // Trigger short press handler
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
}
</style>
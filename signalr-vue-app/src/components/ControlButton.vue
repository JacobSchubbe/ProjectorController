<template>
  <button
      :class="[styleClass]"
      :disabled="disabled"
      @mousedown="startPress"
      @mouseup="endPress.bind(this, false)"
      @mouseleave="endPress.bind(this, true)"
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
    };
  },
  methods: {
    startPress(): void {
      // Reset long press flag
      this.isLongPress = false;

      // Start timer to detect long press
      this.pressTimer = window.setTimeout(() => {
        this.isLongPress = true;
        this.onClick(true); // Call parent function with true for long press
        this.pressTimer = null; // Reset timer
      }, 800); // Long press threshold (800ms)
    },
    endPress(isCancelled: boolean): void {
      // If the timer is active and a long press hasn't triggered yet
      if (this.pressTimer) {
        window.clearTimeout(this.pressTimer); // Clear the timeout
        this.pressTimer = null; // Reset the timer

        // If it's not cancelled and not a long press, trigger a short press
        if (!this.isLongPress && !isCancelled) {
          this.onClick(false); // Call parent function with false for short press
        }
      }
    },
    cancelPress(): void {
      // If mouse moves away from button, cancel the press detection
      if (this.pressTimer) {
        window.clearTimeout(this.pressTimer);
        this.pressTimer = null;
      }
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
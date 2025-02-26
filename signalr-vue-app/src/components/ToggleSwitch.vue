<template>
  <div class="toggle-switch">
    <label class="switch">
      <input
        type="checkbox"
        :checked="isChecked"
        :disabled="disabled"
        @change="onToggle"
      >
      <span class="slider"></span>
    </label>
  </div>
</template>

<script lang="ts">

import { defineComponent } from "vue";

export default defineComponent({
  name: "ToggleSwitch",
  props: {
    isChecked: {
      type: Boolean,
      required: true, // Ensures the checked state is passed
    },
    disabled: {
      type: Boolean,
      default: false, // Makes the toggle switch disableable
    },
  },
  emits: ["update:isChecked"], // Used for two-way binding
  methods: {
    onToggle(event: Event) {
      const target = event.target as HTMLInputElement;
      this.$emit("update:isChecked", target.checked); // Emit updated checked state
    },
  },
});
</script>

<style scoped>
/* Switch container */
.switch {
  position: relative;
  display: inline-block;
  width: min(13vw, 13vh);
  height: min(5.5vw, 5.5vh);
}

/* Hide default HTML checkbox */
.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider::before {
  position: absolute;
  content: "";
  height: min(5vw, 5vh);
  width: min(5vw, 5vh);
  background-color: white;
  transition: 0.4s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: #28a745; /* Green when enabled */
}

input:checked + .slider::before {
  transform: translate3d(calc(min(12vw, 12vh) - min(5.5vw, 5.5vh)), calc(min(0.5vw, 0.5vh)), 0);
}

input:disabled + .slider {
  background-color: #ccc;
  cursor: not-allowed;
  opacity: 0.6;
}
</style>
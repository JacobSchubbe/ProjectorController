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
  width: 60px;
  height: 34px;
}

/* Hide default HTML checkbox */
.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

/* Slider styles */
.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #ccc;
  transition: 0.4s;
  border-radius: 34px;
}

.slider::before {
  position: absolute;
  content: "";
  height: 26px;
  width: 26px;
  left: 4px;
  bottom: 4px;
  background-color: white;
  transition: 0.4s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: #28a745; /* Green when enabled */
}

input:checked + .slider::before {
  transform: translateX(26px); /* Move handle to the right */
}

input:disabled + .slider {
  background-color: #ccc;
  cursor: not-allowed;
  opacity: 0.6;
}
</style>
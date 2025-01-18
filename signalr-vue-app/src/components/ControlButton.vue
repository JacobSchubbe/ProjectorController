<template>
  <button
      :class="[styleClass]"
      :disabled="disabled"
      @click="onClick"
  >
    <slot></slot>
  </button>
</template>

<script lang="ts">
export default {
  name: "ControlButton",
  props: {
    onClick: {
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
};
</script>

<style scoped>
button {
  margin: 10px;
  padding: 10px;
}
.control-button {
  background-color: #007bff;
  color: white;
  /* Additional button styles here */
}
.control-button:disabled {
  background-color: #ccc;
  color: #666;
  pointer-events: none;
}
</style>
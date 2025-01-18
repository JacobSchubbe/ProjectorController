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
</style>
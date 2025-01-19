<template>
  <button
      class="media-button"
      :disabled="disabled"
      @click="onClick"
  >
    <img :src="icon" :alt="alt" class="media-icon" />
  </button>
</template>

<script lang="ts">
export default {
  name: "MediaButton",
  props: {
    icon: {
      type: String,
      required: true,
    },
    alt: {
      type: String,
      default: "",
    },
    onClick: {
      type: Function,
      required: true,
    },
    disabled: {
      type: Boolean,
      default: false,
    },
  },
};
</script>

<style scoped>
.media-button {
  background-color: transparent;
  border: none;
  padding: 0;
}
.media-icon {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
</style>
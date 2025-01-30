<template>
  <div class="apps-grid">
    <!-- Dynamically Render Apps -->
    <MediaButton
        v-for="(app, index) in apps"
        :key="index"
        :icon="app.icon"
        :alt="app.alt"
        class="control-button media-button"
        :onClick="() => handleClick(app.action)"
        :disabled="buttonDisabled"
    />
  </div>
</template>

<script lang="ts" setup>
import MediaButton from "@/components/MediaButton.vue";
import { defineProps } from "vue";

// Props
defineProps({
  buttonDisabled: Boolean, // Passed down from the parent component
  handleClick: Function,   // Function to handle button clicks
  apps: Array,              // Array of app definitions (dynamically passed)
  availableHeight: Number,
});
</script>

<style scoped>
/* Grid Layout for Apps */
.apps-grid {
  width: 100%;
  max-width: 100%; /* Ensure the grid doesn't overflow */
  box-sizing: border-box; /* Include padding in the width */
  display: grid; /* Enable grid layout */
  grid-template-columns: repeat(auto-fill, minmax(80px, 1fr)); /* Ensure consistent spacing */
  gap: 0; /* Increase gap for better distribution */
  justify-items: stretch; /* Center items in their grid cells for a clean layout */
  margin: 0;
  padding: 0;
  overflow: hidden; /* Prevent overflowing content */
}

.media-button {
  background-color: transparent;
  border: none;
  box-sizing: border-box; /* Prevent overflow caused by padding or borders */
  margin: 0;
  padding: 10px;
  width: 100%;
  height: auto;
  cursor: pointer;
}

.media-button img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

</style>
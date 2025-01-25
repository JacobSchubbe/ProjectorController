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
  display: grid; /* Enable grid layout */
  grid-template-columns: repeat(auto-fit, minmax(80px, 1fr)); /* Flexible columns */
  gap: 10px; /* Spacing between apps */
  justify-items: center; /* Center items in their grid cells for a clean layout */
  padding: 10px; /* Add spacing around the grid */
}

.media-button {
  background-color: transparent;
  border: none;
  padding: 0;
  width: 80px;
  height: 80px;
  cursor: pointer;
}

.media-button img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
</style>
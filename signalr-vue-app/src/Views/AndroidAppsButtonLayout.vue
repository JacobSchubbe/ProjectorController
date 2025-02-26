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

.media-button {
  background-color: transparent; /* Make the button background fully transparent */
  border: none; /* Remove borders for a cleaner visual */
  padding: 0; /* Remove extra spacing around the image */
  display: flex; /* Use flexbox for proper content alignment */
  justify-content: center;
  align-items: center;
  width: 20vw;
  cursor: pointer;
  overflow: hidden; /* Clip any overflow from the image */
}

.media-button:disabled {
  opacity: 0.6;
  pointer-events: none; /* Disable pointer interactions when disabled */
}

.media-icon {
  width: 100%; /* Ensure image completely fills the button */
  height: 100%; /* Scale image to fill height */
  object-fit: cover; /* Make sure image covers the entire button evenly */
  pointer-events: none; /* Ensure clicking on the image still triggers the button */
}

/* Grid Layout for Apps */
.apps-grid {
  width: 100%;
  max-width: 100%; /* Ensure the grid doesn't overflow */
  box-sizing: border-box; /* Include padding in the width */
  display: grid; /* Enable grid layout */
  grid-template-columns: repeat(auto-fill, minmax(15vw, 1fr)); /* Ensure consistent spacing */
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
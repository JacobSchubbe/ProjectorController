<template>
  <div class="volume-slider-container">
    <!-- Volume Down Button -->
    <ControlButton
        :disabled="isDisabled"
        :onClick="decreaseVolume"
        class="volume-button"
    >
      Volume -
    </ControlButton>

    <!-- Slider -->
    <input
        type="range"
        min="0"
        max="40"
        v-model="internalVolume"
        class="volume-slider"
        :disabled="isDisabled"
        @input="handleSliderChange"
    />

    <!-- Volume Up Button -->
    <ControlButton
        :disabled="isDisabled"
        :onClick="increaseVolume"
        class="volume-button"
    >
      Volume +
    </ControlButton>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, watch } from "vue";
import ControlButton from "@/components/ControlButton.vue";

export default defineComponent({
  name: "VolumeSlider",
  components: { ControlButton },

  props: {
    modelValue: {
      type: Number,
      required: true, // Bind the volume level from the parent component
    },
    isDisabled: {
      type: Boolean,
      default: false, // Disable control if needed (e.g., if projector is off)
    },
    onVolumeChange: {
      type: Function,
      default: null,
    },
  },

  setup(props) {
    const internalVolume = ref(props.modelValue);
    const incrementSize = 1;
    
    const increaseVolume = () => {
      if (props.isDisabled) return;
      if (internalVolume.value < 40) {
        internalVolume.value += incrementSize;
      }
      else {
        internalVolume.value = 40;
      }
      props.onVolumeChange?.(internalVolume.value);
    };

    const decreaseVolume = () => {
      if (props.isDisabled) return;
      if (internalVolume.value > 0) {
        internalVolume.value -= incrementSize;
      }
      else {
        internalVolume.value = 0;
      }  
      props.onVolumeChange?.(internalVolume.value);
    };

    const handleSliderChange = () => {
      props.onVolumeChange?.(Number(internalVolume.value));
    };

    // Sync changes from parent if `modelValue` updates
    watch(() => props.modelValue, (newValue) => {
      console.log(`Volume changed from parent: ${newValue}`);
      internalVolume.value = newValue;
    });

    return {
      internalVolume,
      increaseVolume,
      decreaseVolume,
      handleSliderChange,
    };
  },
});
</script>

<style scoped>
.volume-slider-container {
  display: flex; /* Horizontal alignment */
  justify-content: center; /* Center items horizontally */
  align-items: center; /* Align items vertically */
  gap: 15px; /* Space between buttons and the slider */
}

.volume-slider {
  width: 150px; /* Adjust slider width as needed */
  accent-color: #1c79cb; /* Customize slider color */
  cursor: pointer;
}

.volume-button {
  background-color: #1c79cb;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px;
  cursor: pointer;
  font-size: 14px;
  font-weight: bold;
  transition: transform 0.2s, background-color 0.3s;
  margin: 10px;
  height: 10vh; /* Or dynamically calculated height */
}

.volume-button:disabled {
  background-color: #ccc;
  color: #666;
  cursor: not-allowed;
  opacity: 0.6;
}

.volume-button:hover:not(:disabled) {
  background-color: #0056b3;
  transform: scale(1.05);
}

.volume-button:active:not(:disabled) {
  transform: scale(0.95);
}
</style>
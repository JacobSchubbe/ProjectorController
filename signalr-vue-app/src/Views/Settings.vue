<template>
  <div class="settings-page">
    <h1>Settings</h1>

    <!-- Dropdown Example -->
    <div class="settings-item">
      <Dropdown
          id="settings-dropdown"
          label="Display Mode:"
          :options="dropdownOptions"
          v-model="localSelectedOption"
          @change="handleDropdownChange"
      />
    </div>

    <!-- Toggle Example -->
    <div class="settings-item">
      <label for="dark-mode-toggle">Dark Mode:</label>
      <Toggle
          id="dark-mode-toggle"
          v-model:isChecked="localIsDarkMode"
      />
    </div>
    <div class="settings-item">
      <label for="dark-mode-toggle">Invert Touch Pad:</label>
      <Toggle
          id="touch-pad-inverted-toggle"
          v-model:isChecked="localIsTouchPadInverted"
      />
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import Dropdown from '../components/DropDown.vue';
import Toggle from '../components/ToggleSwitch.vue';
import * as projectorConstants from '@/Constants/ProjectorConstants';
import { SignalRService } from "./../SignalRServiceManager";

export default defineComponent({
  name: 'SettingsPage',
  components: {
    Dropdown,
    Toggle,
  },
  props: {
    selectedOption: {
      type: Number,
      required: true,
      default: -1,
    },
    isDarkMode: {
      type: Boolean,
      required: true,
      default: false,
    },
    isTouchPadInverted: {
      type: Boolean,
      required: true,
      default: false,
    },
    buttonDisabled: {
      type: Boolean,
      required: true,
    },
    SignalRInstance: {
      type: SignalRService,
      required: true,
    },
  },
  data() {
    return {
      dropdownOptions: [
        { value: projectorConstants.ProjectorCommands.ImageControlCinemaMode, label: 'Cinema' },
        { value: projectorConstants.ProjectorCommands.ImageControlBrightCinemaMode, label: 'Bright Cinema' },
        { value: projectorConstants.ProjectorCommands.ImageControlDynamicMode, label: 'Dynamic' },
        { value: projectorConstants.ProjectorCommands.ImageControlGameMode, label: 'Game' },
      ],
      localSelectedOption: this.selectedOption,
      localIsDarkMode: this.isDarkMode,
      localIsTouchPadInverted: this.isTouchPadInverted,
    };
  },
  watch: {
    selectedOption(newValue) {
      this.localSelectedOption = newValue;
    },
    localIsDarkMode(newValue) {
      this.$emit('update:isDarkMode', newValue);
    },
    localIsTouchPadInverted(newValue) {
      this.$emit('update:isTouchPadInverted', newValue);
    },
  },
  methods: {
    handleDropdownChange(): void {
      console.log(`Selected Input: ${this.localSelectedOption}`);
      this.SignalRInstance.sendProjectorCommand(this.localSelectedOption);
      console.log(`Command sent: ${this.localSelectedOption}`);
    },
    queryItemStatus(): void {
        console.log('Querying item statuses from SignalR...');
        this.SignalRInstance.queryForProjectorSettings();
    },
  },
  mounted() {
    if (this.buttonDisabled === false) {
      this.queryItemStatus();
    }
  },
});
</script>
<style scoped>
/* Basic styles for the settings page */
.settings-page {
  padding: 20px;
  font-family: Arial, sans-serif;
}

.settings-item {
  margin-bottom: 20px;
}

label {
  display: block;
  font-weight: bold;
  margin-bottom: 5px;
}

h1 {
  margin-bottom: 20px;
}
</style>
<template>
  <div>
    <label>{{ label }}</label>
    <select v-model="selected" @change="onChange">
      <option v-for="option in options" :key="option.value" :value="option.value">
        {{ option.label }}
      </option>
    </select>
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";

export default defineComponent({
  name: "DropDown",
  props: {
    label: {
      type: String,
      required: true,
    },
    options: {
      type: Array as PropType<Array<{ label: string; value: string | number }>>,
      required: true,
    },
    modelValue: {
      type: [String, Number],
      default: null,
    },
  },
  emits: ["update:modelValue", "change"],
  computed: {
    selected: {
      get(): string | number | null {
        return this.modelValue;
      },
      set(value: string | number) {
        this.$emit("update:modelValue", value);
        this.$emit("change", value);
      },
    },
  },
});
</script>
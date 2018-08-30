<template>
  <v-combobox label="Thể loại"
              @change="onSelect"
              v-model="selectedCategory"
              :items="items"
              :loading="loading"
  >
  </v-combobox>
</template>

<script>
  import {StoreBoundSelectMixin} from "../mixin";

  export default {
    name: "CategoryInput",
    mixins: [
      StoreBoundSelectMixin({
        alias: 'tag',
        itemsPath: 'tagCategories',
        loadingPath: 'tagCategoryLoading',
        getItemPath: 'fetchCategories',
      })
    ],
    data() {
      return {
        selectedCategory: ''
      }
    },
    watch:{
      value:{
        immediate: true,
        handler(val){
          this.selectedCategory = val;
        }
      }
    },
    mounted() {
      this.selectedCategory = this.value;
    }
  }
</script>

<style scoped>

</style>

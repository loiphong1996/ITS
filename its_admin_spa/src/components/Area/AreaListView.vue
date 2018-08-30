<template>
  <v-container id="content" fluid>
    <v-layout row pa-3 style="background-color: white" elevation-4>
      <v-flex xs12>
        <span class="title">Quản lí Khu vực</span>
        <v-divider class="my-3"></v-divider>
        <!--Content start-->
        <v-card-title>
          <v-text-field
            v-model="searchInput"
            v-on:keyup.enter="onSearchEnter"
            append-icon="search"
            label="Tìm"
            single-line
            hide-details>
          </v-text-field>
          <v-spacer></v-spacer>
          <v-btn color="primary"
                 :to="{name:'AreaCreate'}">
            Tạo mới
          </v-btn>
        </v-card-title>
        <v-data-table
          :items="items"
          :total-items="total"
          :pagination.sync="pagination"
          :headers="headers"
          :loading="loading">
          <template slot="items" slot-scope="props">
            <td>{{ props.item.name }}</td>
            <td>{{ props.item.locationCount }}</td>
            <td>{{ props.item.planCount ? props.item.planCount : 0 }}</td>
            <td>{{ props.item.questionCount ? props.item.questionCount  : 0}}</td>
            <td class="justify-center layout px-0">
              <router-link :to="{name:'AreaEdit', query:{id:props.item.id}}">
                <v-icon
                  small
                  class="mr-2"
                  color="green">
                  edit
                </v-icon>
              </router-link>
              <!--<a>-->
                <!--<v-icon-->
                  <!--small-->
                  <!--color="red"-->
                  <!--@click="onDeleteClick(props.item)">-->
                  <!--delete-->
                <!--</v-icon>-->
              <!--</a>-->
            </td>
          </template>
        </v-data-table>
        <!--Content end-->
      </v-flex>
    </v-layout>
    <ErrorDialog v-bind="error" v-on:close="error.dialog = false"/>
    <SuccessDialog v-bind="success" v-on:close="success.dialog = false"/>
  </v-container>
</template>
<!--
TODO
BASIC
name
description
country

ADVANCE
photo
question
-->
<script>
  import {
    ErrorDialog,
    SuccessDialog
  } from "../../common/block";

  export default {
    name: "AreaListView",
    components: {ErrorDialog, SuccessDialog},
    data() {
      return {
        //TABLE START
        loading: true,
        items: [],
        headers: [
          {text: 'Tên', value: 'name'},
          {text: 'Số địa điểm', value: 'locationCount'},
          {text: 'Số chuyến đi', value: 'planCount'},
          {text: 'Số câu hỏi', value: 'questionCount'},
          {text: 'Hành động', value: 'id', sortable: false},
        ],
        pagination: {},
        total: undefined,
        searchInput: '',
        //TABLE END
        //DIALOG START
        error: {
          dialog: false,
          title: '',
          message: ''
        },
        success: {
          dialog: false,
          title: '',
          message: ''
        }
        //DIALOG END;
      }
    },
    watch: {
      pagination: {
        //Do not use arrow funcs
        handler: function () {
          this.loadData();
        },
        deep: true
      }
    },
    methods: {
      loadData() {
        this.loading = true;
        this.$store.dispatch('area/getAll', {
          search: this.searchInput,
          pagination: this.pagination
        })
          .then(data => {
            console.debug('area-loadData',data);
            this.items = data.list;
            this.total = data.total;
            this.loading = false;
          })
          .catch(error => {
            console.error(error);
            this.error = {
              dialog: true,
              title: 'Chú ý',
              message: error.message
            };
            // console.error('loadData', error);
          })
      },
      onSearchEnter() {
        this.pagination.page = 1;
        this.loadData();
      },
      onDeleteClick(item) {
        this.loading = true;
        this.$store.dispatch('area/delete', {...item})
          .then(area => {
            this.success = {
              dialog: true,
              message: `Xóa thành công khu vực ${area.name}`
            };
            this.loading = false;
          })
          .catch(reason => {
            this.error = {
              dialog: true,
              ...reason
            }
          })
      }
    },

  }
</script>

<style scoped>

</style>

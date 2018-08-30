<template>
  <v-container id="content" fluid>
    <v-layout row pa-3 style="background-color: white" elevation-4>
      <v-flex xs12>
        <span class="title">Quản lí tài khoản</span>
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
          <!--<v-spacer></v-spacer>-->
          <!--<v-btn color="primary"-->
                 <!--:to="{name:'AccountCreate'}">-->
            <!--Tạo mới-->
          <!--</v-btn>-->
        </v-card-title>
        <v-data-table
          :items="items"
          :total-items="total"
          :pagination.sync="pagination"
          :headers="headers"
          :loading="loading">
          <template slot="items" slot-scope="props">
            <td>{{ props.item.name }}</td>
            <td>{{ props.item.emailAddress }}</td>
            <td>{{ props.item.phoneNumber }}</td>
            <td>{{ props.item.birthdate }}</td>
            <td>{{ props.item.address }}</td>
            <!--<td>-->

              <!--<v-chip outline disabled label color="green"-->
                      <!--v-if="!props.item.ban">-->
                <!--Hoạt động-->
              <!--</v-chip>-->
              <!--<v-chip outline disabled label color="red"-->
                      <!--v-if="props.item.ban">-->
                <!--Khóa-->
              <!--</v-chip>-->
            <!--</td>-->
            <td class="justify-center layout px-0">
              <router-link :to="{name:'AccountEdit', query:{id:props.item.id}}">
                <v-icon
                  small
                  class="mr-2"
                  color="green">
                  edit
                </v-icon>
              </router-link>
              <a>
                <v-icon
                  small
                  color="red"
                  @click="banClick(props.item)">
                  fas fa-ban
                </v-icon>
              </a>
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

<script>
  import {
    ErrorDialog,
    SuccessDialog
  } from "../../common/block";


  export default {
    name: "AccountListView",
    components: {ErrorDialog, SuccessDialog},
    data() {
      return {
        //TABLE START
        loading: true,
        items: [],
        headers: [
          {text: 'Tên', value: 'name'},
          {text: 'Email', value: 'emailAddress'},
          {text: 'Điện thoại', value: 'phoneNumber'},
          {text: 'Ngày sinh', value: 'birthdate'},
          {text: 'Địa chỉ', value: 'address'},
          {text: 'Khóa', value: 'ban'},
          // {text: 'Hành động', value: 'id', sortable: false},
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
        //DIALOG END
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
    mounted() {
      this.loadData();
    },
    methods: {
      loadData() {
        this.loading = true;
        this.$store.dispatch('account/getAll', {
          search: this.searchInput,
          pagination: this.pagination
        })
          .then(data => {
            this.items = data.accounts;
            this.total = data.total;
            this.loading = false;
          })
          .catch(error => {
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
      banClick(item) {
        this.$store.dispatch('account/ban')
          .then(value => {
            this.loadData();
          })
        // this.loading = true;
        // const handleSuccess = (account) => {
        //   item = {
        //     ...account
        //   };
        //   let statusText = account.ban ? 'Khóa' : 'Hoạt động';
        //   this.success = {
        //     dialog: true,
        //     message: `Tài khoản ${account.name} chuyển thành ${statusText}`
        //   };
        //
        //   this.loading = false;
        // };
        // const handleError = (error) => {
        //   this.error = {
        //     dialog: true,
        //     message: error.message
        //   };
        //   this.loading = false;
        // };
        // if (item.ban) {
        //   this.$store.dispatch('account/unBan', {
        //     id: item.id
        //   }).then(handleSuccess)
        //     .catch(handleError)
        // } else {
        //   this.$store.dispatch('account/ban', {
        //     id: item.id
        //   }).then(handleSuccess)
        //     .catch(handleError)
        // }

      },
    }
  }
</script>

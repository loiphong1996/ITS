<template>
  <v-content>
    <ParallaxHeader
      src="static/pexels-photo-490411.jpeg"
      text="Thông tin cá nhân"
    />
    <v-container v-if="!pageLoading">
      <v-layout my-3 mx-2 column>
        <!--Separate edit view-->
        <v-flex>
          <PictureInput
            v-model="input.avatar"
            width="300"
            height="300"
            text="Ảnh đại diện"
          />
        </v-flex>
        <v-flex>
          <v-layout column>
            <v-text-field label="Tên" PictureInput
                          readonly
                          v-model="input.name"/>
            <v-text-field label="Email"
                          readonly
                          v-model="input.emailAddress"/>
            <v-text-field label="Điện thoại"
                          readonly
                          v-model="input.phoneNumber"/>
            <v-text-field label="Địa chỉ"
                          readonly
                          v-model="input.address"/>
            <v-text-field label="Ngày sinh"
                          type="date"
                          readonly
                          v-model="input.birthdate"/>
          </v-layout>
        </v-flex>
        <v-flex>
          <!--<v-btn color="success">Cập nhật</v-btn>-->
          <!--<v-btn color="secondary" :loading="changePasswordLoading">Đổi mật khẩu</v-btn>-->
          <v-btn color="secondary" @click="signout">Đăng xuất</v-btn>
        </v-flex>
        <v-flex style="height: 30vh">
          <!--Holder-->
        </v-flex>
      </v-layout>
      <!--CHANGE PASSWORD DIALOG-->
      <v-dialog v-model="changePasswordDialog.dialog" max-width="550">
        <v-card>
          <v-card-title class="white--text light-blue darken-2 title">
            Thay đổi mật khẩu
          </v-card-title>
          <v-card-text>
            <v-layout column>
              <v-text-field label="Mật khẩu" v-model="changePasswordDialog.newPassword">

              </v-text-field>
              <v-text-field label="Nhập lại mật khảu" v-model="changePasswordDialog.reNewPassword">

              </v-text-field>
            </v-layout>
          </v-card-text>
          <v-card-actions>
            <v-layout>
              <v-btn color="primary"
                     :loading="changePasswordLoading">
                Xác nhận
              </v-btn>
              <v-btn color="secondary" @click="emailInputDialog = false">
                Hủy
              </v-btn>
            </v-layout>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-container>
    <v-container class="text-xs-center" v-else>
      <v-progress-circular indeterminate size="40" color="primary"></v-progress-circular>
    </v-container>
    <v-container>
      <!--Holder-->
    </v-container>
  </v-content>
</template>

<script>
  import PictureInput from "../../common/input/PictureInput";
  import ParallaxHeader from "../../common/layout/ParallaxHeader";
  import {mapState} from 'vuex'

  export default {
    name: "PersonalView",
    components: {
      ParallaxHeader,
      PictureInput
    },
    data() {
      return {
        editMode: true,
        loading: {
          page: true
        },
        input: {
          avatar: undefined,
          name: undefined,
          emailAddress: undefined,
          phoneNumber: undefined,
          address: undefined,
          birthdate: undefined
        },
        changePasswordDialog: {
          //CHANGE PASSWORD
          newPassword: undefined,
          reNewPassword: undefined,
          dialog: false,
        }
      }
    },
    computed: {
      ...mapState('user', {
        currentAccount: 'current',
        pageLoading: state => state.loading.currentUser
      }),
      ...mapState('account', {
        changePasswordLoading: state => state.loading.changePsssword,
      }),
    },
    mounted() {
      this.$store.dispatch('user/fetchCurrentInfo')
        .then(() => {
          this.setInputs(this.currentAccount);
        });
      this.loading.page = false
    },
    methods: {
      setInputs(account) {
        this.input.avatar = account.avatar;
        this.input.name = account.name;
        this.input.emailAddress = account.emailAddress;
        this.input.phoneNumber = account.phoneNumber;
        this.input.address = account.address;
        this.input.birthdate = account.birthdate;
      },
      updateProfile(){

      },
      signout() {
        this.$store.dispatch('authenticate/logout');
        this.$store.commit('authenticate/nullToken');
        this.$router.push({
          name: 'Home'
        });
      },
      changePassword() {

      }
    }
  }
</script>

<style scoped>

</style>

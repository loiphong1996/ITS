new Vue({
    store,
    el: "div.result_item_box",
    data() {
        return {
            nameInput: '',
            startDateInput: '',
            endDateInput: ''
        }
    },
    methods: {
        createPlan() {
            //Validate plan inputs
            let payload = {
                name: this.nameInput,
                startDate: this.startDateInput,
                endDate: this.endDateInput,
            };
            this.$store.dispatch('createPlan',payload)
        }
    }
});
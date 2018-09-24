(function () {
        $(function () {
                var bearerAuthUI =
                        '<div class="input"><input placeholder="Bearer Token" id="input_token" name="token" type="text" size="10"></div>'
                $(bearerAuthUI).insertBefore('#api_selector div.input:last-child');
                $("#input_apiKey").hide();

                $('#input_token').change(addAuthorization);
        });

        function addAuthorization() {
            var token = $('#input_token').val();
                if (token && token.trim() !== "") {
                    var bearerAuth = new SwaggerClient.ApiKeyAuthorization('Authorization', 'bearer ' + token, 'header');
                    window.swaggerUi.api.clientAuthorizations.add("apiKey", bearerAuth);
                    console.log("authorization added: username = " + token);
                }
        }
})();
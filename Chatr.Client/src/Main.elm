port module Main exposing (main)

import Browser
import Html exposing (Html, button, div, h1, h4, input, text)
import Html.Attributes exposing (class, placeholder, value)
import Html.Events exposing (onClick, onInput)


port login : ( String, String ) -> Cmd msg


port onLoggedIn : (Bool -> msg) -> Sub msg


port register : ( String, String ) -> Cmd msg


port joinRoom : String -> Cmd msg


port leaveRoom : String -> Cmd msg


port sendToRoom : ( String, String, String ) -> Cmd msg


port onRoomMessage : (String -> msg) -> Sub msg


subscriptions : Model -> Sub Msg
subscriptions _ =
    Sub.batch [ onLoggedIn LoggedIn, onRoomMessage Received ]


main : Program () Model Msg
main =
    Browser.document
        { init = init
        , update = update
        , subscriptions = subscriptions
        , view = \model -> { title = "ChatR", body = [ view model ] }
        }


type Viewing
    = LoggingIn
    | Registering
    | Chatting


type alias Model =
    { viewing : Viewing
    , userName : String
    , pass : String
    , message : String
    , rooms : List String
    , currentRoom : String
    , log : List String
    }


init : () -> ( Model, Cmd Msg )
init _ =
    ( { viewing = LoggingIn
      , userName = ""
      , pass = ""
      , message = ""
      , rooms = [ "Alpha", "Beta" ]
      , currentRoom = "Alpha"
      , log = []
      }
    , Cmd.none
    )


type Msg
    = UpdateUserName String
    | UpdatePass String
    | Login
    | LoggedIn Bool
    | GotoRegistering
    | Register
    | UpdateMessage String
    | JoinRoom String
    | SendToRoom
    | Received String


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        UpdateUserName str ->
            ( { model | userName = str }, Cmd.none )

        UpdatePass str ->
            ( { model | pass = str }, Cmd.none )

        Login ->
            ( model, login ( model.userName, model.pass ) )

        LoggedIn success ->
            ( { model | viewing = Chatting }, Cmd.none )

        GotoRegistering ->
            ( { model | viewing = Registering }, Cmd.none )

        Register ->
            ( model, register ( model.userName, model.pass ) )

        UpdateMessage m ->
            ( { model | message = m }, Cmd.none )

        JoinRoom room ->
            ( { model | currentRoom = room }, joinRoom room )

        SendToRoom ->
            ( { model | message = "" }
            , sendToRoom ( model.currentRoom, model.userName, model.message )
            )

        Received str ->
            ( { model | log = str :: model.log }, Cmd.none )


view : Model -> Html Msg
view model =
    case model.viewing of
        LoggingIn ->
            loginView model

        Registering ->
            registerView model

        Chatting ->
            roomsView model


loginView : Model -> Html Msg
loginView model =
    div []
        [ h1 [] [ text "ChatR" ]
        , h4 [] [ text "Log in" ]
        , input [ placeholder "User", value model.userName, onInput UpdateUserName ] []
        , input [ placeholder "Password", value model.pass, onInput UpdatePass ] []
        , button [ onClick Login ] [ text "Login" ]
        , div [] [ text "Or ", button [ onClick GotoRegistering ] [ text "Register" ] ]
        ]


registerView : Model -> Html Msg
registerView model =
    div []
        [ h1 [] [ text "ChatR" ]
        , h4 [] [ text "Register" ]
        , input [ placeholder "User", value model.userName, onInput UpdateUserName ] []
        , input [ placeholder "Password", value model.pass, onInput UpdatePass ] []
        , button [ onClick Register ] [ text "Register" ]
        ]


roomsView : Model -> Html Msg
roomsView model =
    let
        roomButton room =
            if room == model.currentRoom then
                button [ class "active-room" ] [ text room ]

            else
                button [ onClick (JoinRoom room) ] [ text room ]

        rooms =
            model.rooms |> List.map roomButton
    in
    div []
        [ h1 [] [ text "ChatR" ]
        , div [] rooms
        , h4 [] [ text <| model.userName ++ ":" ]
        , input [ placeholder "Message", value model.message, onInput UpdateMessage ] []
        , button [ onClick SendToRoom ] [ text "Send to Room" ]
        , div [ class "log" ]
            (List.map (\m -> div [] [ text m ]) model.log)
        ]

namespace MAIGame.Echo
{
    public enum EchoSwapFailureReason
    {
        NoPlayerReference,
        NoActiveEcho,
        CooldownActive,
        GameNotPlaying,
        PlayerNotGrounded,
        PlayerFalling,
        InvalidPlayerDestination,
        InvalidEchoDestination
    }
}

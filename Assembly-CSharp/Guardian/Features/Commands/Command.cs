﻿namespace Guardian.Features.Commands
{
    public abstract class Command : Feature
    {
        public string Usage;
        public bool MasterClient;

        public Command(string name, string[] aliases, string usage, bool masterClient) : base(name, aliases)
        {
            this.Usage = usage;
            this.MasterClient = masterClient;
        }

        public abstract void Execute(InRoomChat irc, string[] args);
    }
}

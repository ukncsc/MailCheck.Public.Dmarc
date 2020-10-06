﻿using System;

namespace MailCheck.Dmarc.Poller.Domain
{
    public abstract class Tag : DmarcEntity
    {
        protected Tag(string value)
        {
            Value = $"{value};";
        }

        public string Value { get; }

        public string Explanation { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}{Environment.NewLine}{nameof(Explanation)}: {Explanation}";
        }
    }
}
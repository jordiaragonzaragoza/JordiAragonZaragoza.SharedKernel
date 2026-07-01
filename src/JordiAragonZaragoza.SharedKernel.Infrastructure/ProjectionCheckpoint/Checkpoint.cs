namespace JordiAragonZaragoza.SharedKernel.Infrastructure.ProjectionCheckpoint
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.Contracts;

    public sealed record class Checkpoint : IDataEntity
    {
        public Checkpoint(
            Guid id,
            ulong position,
            DateTimeOffset checkpointedAtOnUtc)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id must not be empty.", nameof(id));
            }

            if (position == default)
            {
                throw new ArgumentException("Position must not be zero.", nameof(position));
            }

            if (checkpointedAtOnUtc == default)
            {
                throw new ArgumentException("CheckpointedAtOnUtc must not be default value.", nameof(checkpointedAtOnUtc));
            }

            this.Id = id;
            this.Position = position;
            this.CheckpointedAtOnUtc = checkpointedAtOnUtc;
        }

        public Guid Id { get; }

        public ulong Position { get; set; }

        public DateTimeOffset CheckpointedAtOnUtc { get; set; }
    }
}